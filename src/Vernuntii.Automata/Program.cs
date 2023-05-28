using Vernuntii.Reactive.Broker;
using Vernuntii.Reactive.Coroutines;
using Vernuntii.Reactive.Coroutines.PingPong;
using Vernuntii.Reactive.Coroutines.Steps;

var eventBroker = new EventBroker();

var coroutineExecutor = new CoroutineExecutorBuilder()
    .AddSteps(new EventStepStore())
    .Build();

coroutineExecutor.Start(new PingCoroutine(eventBroker).PongWhenPinged);
coroutineExecutor.Start(new PongCoroutine(eventBroker).PingWhenPonged);
await Task.Delay(500);
await eventBroker.EmitAsync(PingCoroutine.Pinged, new Ping(1));
await coroutineExecutor.WhenAll();
