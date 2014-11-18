using System;
using NUnit.Framework;
using WorkoutWotch.Services.State;
using System.Threading.Tasks;
using Kent.Boogaart.PCLMock;
using WorkoutWotch.UnitTests.Services.Logger.Mocks;
using WorkoutWotch.UnitTests.Services.State.Mocks;

namespace WorkoutWotch.UnitTests.Services.State
{
    [TestFixture]
    public class StateServiceFixture
    {
        [Test]
        public async Task save_async_executes_all_tasks_returned_by_saved_callbacks()
        {
            var service = new StateService(new BlobCacheMock(), new LoggerServiceMock(MockBehavior.Loose));
            var firstExecuted = false;
            var secondExecuted = false;
            service.RegisterSaveCallback(_ => Task.Run(() => firstExecuted = true));
            service.RegisterSaveCallback(_ => Task.Run(() => secondExecuted = true));

            await service.SaveAsync();

            Assert.True(firstExecuted);
            Assert.True(secondExecuted);
        }

        [Test]
        public async Task save_async_does_not_fail_if_a_save_callback_fails()
        {
            var service = new StateService(new BlobCacheMock(), new LoggerServiceMock(MockBehavior.Loose));
            service.RegisterSaveCallback(_ => Task.Run(() => { throw new Exception("Failed"); }));

            await service.SaveAsync();
        }

        [Test]
        public void register_save_callback_throws_if_save_task_factory_is_null()
        {
            var service = new StateService(new BlobCacheMock(), new LoggerServiceMock(MockBehavior.Loose));
            Assert.Throws<ArgumentNullException>(() => service.RegisterSaveCallback(null));
        }

        [Test]
        public async Task register_save_callback_returns_a_registration_handle_that_unregisters_the_callback_when_disposed()
        {
            var service = new StateService(new BlobCacheMock(), new LoggerServiceMock(MockBehavior.Loose));
            var firstExecuted = false;
            var secondExecuted = false;
            var handle = service.RegisterSaveCallback(_ => Task.Run(() => firstExecuted = true));
            service.RegisterSaveCallback(_ => Task.Run(() => secondExecuted = true));

            handle.Dispose();

            await service.SaveAsync();

            Assert.False(firstExecuted);
            Assert.True(secondExecuted);
        }
    }
}

