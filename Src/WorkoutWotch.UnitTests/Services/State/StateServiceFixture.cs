using System.Reactive.Linq;
using System.Reactive;

namespace WorkoutWotch.UnitTests.Services.State
{
    using System;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using NUnit.Framework;
    using WorkoutWotch.Services.State;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.State.Mocks;

    [TestFixture]
    public class StateServiceFixture
    {
        [Test]
        public void get_async_throws_if_key_is_null()
        {
            var service = new StateService(new BlobCacheMock(), new LoggerServiceMock(MockBehavior.Loose));
            Assert.Throws<ArgumentNullException>(() => service.GetAsync<string>(null));
        }

        [Test]
        public void get_async_forwards_the_call_onto_the_blob_cache()
        {
            var blobCache = new BlobCacheMock();
            blobCache.When(x => x.GetAsync(It.IsAny<string>())).Return(Observable.Return(new byte[0]));
            var service = new StateService(blobCache, new LoggerServiceMock(MockBehavior.Loose));
            service.GetAsync<string>("some key");

            // we don't verify the specific key because Akavache does some key manipulation internally
            blobCache.Verify(x => x.GetAsync(It.IsAny<string>())).WasCalledExactlyOnce();
        }

        [Test]
        public void set_async_throws_if_key_is_null()
        {
            var service = new StateService(new BlobCacheMock(), new LoggerServiceMock(MockBehavior.Loose));
            Assert.Throws<ArgumentNullException>(() => service.SetAsync<string>(null, "foo"));
        }

        [Test]
        public void set_async_forwards_the_call_onto_the_blob_cache()
        {
            var blobCache = new BlobCacheMock(MockBehavior.Loose);
            blobCache.When(x => x.Insert(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DateTimeOffset?>())).Return(Observable.Return(Unit.Default));
            var service = new StateService(blobCache, new LoggerServiceMock(MockBehavior.Loose));
            service.SetAsync<string>("some key", "some value");

            // we don't verify the specific key because Akavache does some key manipulation internally
            blobCache.Verify(x => x.Insert(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DateTimeOffset?>())).WasCalledExactlyOnce();
        }

        [Test]
        public void remove_async_throws_if_key_is_null()
        {
            var service = new StateService(new BlobCacheMock(), new LoggerServiceMock(MockBehavior.Loose));
            Assert.Throws<ArgumentNullException>(() => service.RemoveAsync<string>(null));
        }

        [Test]
        public void remove_async_forwards_the_call_onto_the_blob_cache()
        {
            var blobCache = new BlobCacheMock(MockBehavior.Loose);
            blobCache.When(x => x.Invalidate(It.IsAny<string>())).Return(Observable.Return(Unit.Default));
            var service = new StateService(blobCache, new LoggerServiceMock(MockBehavior.Loose));
            service.RemoveAsync<string>("some key");

            // we don't verify the specific key because Akavache does some key manipulation internally
            blobCache.Verify(x => x.Invalidate(It.IsAny<string>())).WasCalledExactlyOnce();
        }

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
        public async Task save_async_ignores_any_null_tasks_returned_by_saved_callbacks()
        {
            var logger = new LoggerMock(MockBehavior.Loose);
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            loggerService.When(x => x.GetLogger(typeof(StateService))).Return(logger);
            var service = new StateService(new BlobCacheMock(), loggerService);
            var firstExecuted = false;
            var secondExecuted = false;
            service.RegisterSaveCallback(_ => Task.Run(() => firstExecuted = true));
            service.RegisterSaveCallback(_ => null);
            service.RegisterSaveCallback(_ => Task.Run(() => secondExecuted = true));

            await service.SaveAsync();

            Assert.True(firstExecuted);
            Assert.True(secondExecuted);
            loggerService.Verify(x => x.GetLogger(typeof(StateService))).WasCalledExactlyOnce();
            logger.Verify(x => x.Error(It.IsAny<string>())).WasNotCalled();
            logger.Verify(x => x.Error(It.IsAny<string>(), It.IsAny<object[]>())).WasNotCalled();
            logger.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>())).WasNotCalled();
        }

        [Test]
        public async Task save_async_does_not_fail_if_a_save_callback_fails()
        {
            var service = new StateService(new BlobCacheMock(), new LoggerServiceMock(MockBehavior.Loose));
            service.RegisterSaveCallback(_ => Task.Run(() => { throw new Exception("Failed"); }));

            await service.SaveAsync();
        }

        [Test]
        public async Task save_async_logs_an_error_if_a_save_callback_fails()
        {
            var logger = new LoggerMock(MockBehavior.Loose);
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);
            loggerService.When(x => x.GetLogger(typeof(StateService))).Return(logger);
            var service = new StateService(new BlobCacheMock(), loggerService);
            service.RegisterSaveCallback(_ => Task.Run(() => { throw new Exception("whatever"); }));

            await service.SaveAsync();

            logger.Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>())).WasCalledExactlyOnce();
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