using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Sql.EventProcessing;

public class EventProcessingHostedService : IHostedService
{
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;

    private readonly DatabaseContextObserver _databaseContextObserver;
    private readonly Type _eventProcessingPipelineType = typeof(EventProcessingPipeline);
    private readonly Type _eventProcessorStartupPipelineType = typeof(EventProcessorStartupPipeline);
    private readonly IPipelineFactory _pipelineFactory;

    public EventProcessingHostedService(IOptions<SqlEventProcessingOptions> sqlEventProcessingOptions, IPipelineFactory pipelineFactory, IDatabaseContextFactory databaseContextFactory, DatabaseContextObserver databaseContextObserver)
    {
        _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(sqlEventProcessingOptions).Value);
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _databaseContextObserver = Guard.AgainstNull(databaseContextObserver);

        _pipelineFactory.PipelineCreated += OnPipelineCreated;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_sqlEventProcessingOptions.ConfigureDatabase)
        {
            return;
        }

        await using (var databaseContext = _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName))
        {
            await databaseContext.ExecuteAsync(new Query($@"
EXEC sp_getapplock @Resource = '{typeof(EventProcessingHostedService).FullName}', @LockMode = 'Exclusive', @LockOwner = 'Session', @LockTimeout = 15000;

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = '{_sqlEventProcessingOptions.Schema}')
BEGIN
    EXEC('CREATE SCHEMA {_sqlEventProcessingOptions.Schema}');
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{_sqlEventProcessingOptions.Schema}].[Projection]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{_sqlEventProcessingOptions.Schema}].[Projection]
    (
	    [Name] [nvarchar](650) NOT NULL,
	    [SequenceNumber] [bigint] NOT NULL,
        CONSTRAINT [PK_Projection] PRIMARY KEY CLUSTERED 
        (
	        [Name] ASC
        )
        WITH 
        (
            PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF
        ) ON [PRIMARY]
    ) ON [PRIMARY]
END

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{_sqlEventProcessingOptions.Schema}].[ProjectionJournal]') AND type in (N'U'))
BEGIN
    CREATE TABLE [{_sqlEventProcessingOptions.Schema}].[ProjectionJournal]
    (
	    [Name] [nvarchar](650) NOT NULL,
	    [SequenceNumber] [bigint] NOT NULL,
	    [DateCompleted] [datetime2](7) NULL,
        CONSTRAINT [PK_ProjectionJournal] PRIMARY KEY CLUSTERED 
        (
	        [Name] ASC,
	        [SequenceNumber] ASC
        )
        WITH 
        (
            PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF
        ) ON [PRIMARY]
    ) ON [PRIMARY]
END

EXEC sp_releaseapplock @Resource = '{typeof(EventProcessingHostedService).FullName}', @LockOwner = 'Session';
"));
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _pipelineFactory.PipelineCreated -= OnPipelineCreated;

        await Task.CompletedTask;
    }

    private void OnPipelineCreated(object? sender, PipelineEventArgs e)
    {
        var pipelineType = e.Pipeline.GetType();

        if (pipelineType == _eventProcessingPipelineType ||
            pipelineType == _eventProcessorStartupPipelineType)
        {
            e.Pipeline.AddObserver(_databaseContextObserver);
        }

        if (pipelineType == _eventProcessorStartupPipelineType)
        {
            e.Pipeline.AddObserver<EventProcessingStartupObserver>();
        }
    }
}