namespace WorkoutWotch.UnitTests.Services.State
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using Kent.Boogaart.PCLMock;
    using WorkoutWotch.Services.State;
    using WorkoutWotch.UnitTests.Services.Logger.Mocks;
    using WorkoutWotch.UnitTests.Services.State.Mocks;
    using Xunit;

    public class StateServiceFixture
    {
        [Fact]
        public void ctor_throws_if_blob_cache_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new StateService(null, new LoggerServiceMock()));
        }

        [Fact]
        public void ctor_throws_if_logger_service_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new StateService(new BlobCacheMock(), null));
        }

        [Fact]
        public async Task get_async_throws_if_key_is_null()
        {
            var sut = new StateServiceBuilder().Build();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.GetAsync<string>(null));
        }

        [Fact]
        public void get_async_forwards_the_call_onto_the_blob_cache()
        {
            var blobCache = new BlobCacheMock();

            blobCache
                .When(x => x.Get(It.IsAny<string>()))
                .Return(Observable.Return(new byte[0]));

            var sut = new StateServiceBuilder()
                .WithBlobCache(blobCache)
                .Build();

            sut.GetAsync<string>("some key");

            // we don't verify the specific key because Akavache does some key manipulation internally
            blobCache
                .Verify(x => x.Get(It.IsAny<string>()))
                .WasCalledExactlyOnce();
        }

        [Fact]
        public async Task set_async_throws_if_key_is_null()
        {
            var sut = new StateServiceBuilder().Build();

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.SetAsync<string>(null, "foo"));
        }

        [Fact]
        public void set_async_forwards_the_call_onto_the_blob_cache()
        {
            var blobCache = new BlobCacheMock(MockBehavior.Loose);

            blobCache
                .When(x => x.Insert(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DateTimeOffset?>()))
                .Return(Observable.Return(Unit.Default));

            var sut = new StateServiceBuilder()
                .WithBlobCache(blobCache)
                .Build();

            sut.SetAsync<string>("some key", "some value");

            // we don't verify the specific key because Akavache does some key manipulation internally
            blobCache
                .Verify(x => x.Insert(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DateTimeOffset?>()))
                .WasCalledExactlyOnce();
        }

        [Fact]
        public void remove_async_throws_if_key_is_null()
        {
            var sut = new StateServiceBuilder().Build();

            Assert.ThrowsAsync<ArgumentNullException>(() => sut.RemoveAsync<string>(null));
        }

        [Fact]
        public void remove_async_forwards_the_call_onto_the_blob_cache()
        {
            var blobCache = new BlobCacheMock(MockBehavior.Loose);

            blobCache
                .When(x => x.Invalidate(It.IsAny<string>()))
                .Return(Observable.Return(Unit.Default));

            var sut = new StateServiceBuilder()
                .WithBlobCache(blobCache)
                .Build();

            sut.RemoveAsync<string>("some key");

            // we don't verify the specific key because Akavache does some key manipulation internally
            blobCache
                .Verify(x => x.Invalidate(It.IsAny<string>()))
                .WasCalledExactlyOnce();
        }

        [Fact]
        public async Task save_async_executes_all_tasks_returned_by_saved_callbacks()
        {
            var sut = new StateServiceBuilder().Build();
            var firstExecuted = false;
            var secondExecuted = false;
            sut.RegisterSaveCallback(_ => Task.Run(() => firstExecuted = true));
            sut.RegisterSaveCallback(_ => Task.Run(() => secondExecuted = true));

            await sut.SaveAsync();

            Assert.True(firstExecuted);
            Assert.True(secondExecuted);
        }

        [Fact]
        public async Task save_async_ignores_any_null_tasks_returned_by_saved_callbacks()
        {
            var logger = new LoggerMock(MockBehavior.Loose);
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);

            loggerService
                .When(x => x.GetLogger(typeof(StateService)))
                .Return(logger);

            var sut = new StateServiceBuilder()
                .WithLoggerService(loggerService)
                .Build();

            var firstExecuted = false;
            var secondExecuted = false;
            sut.RegisterSaveCallback(_ => Task.Run(() => firstExecuted = true));
            sut.RegisterSaveCallback(_ => null);
            sut.RegisterSaveCallback(_ => Task.Run(() => secondExecuted = true));

            await sut.SaveAsync();

            Assert.True(firstExecuted);
            Assert.True(secondExecuted);

            loggerService
                .Verify(x => x.GetLogger(typeof(StateService)))
                .WasCalledExactlyOnce();

            logger
                .Verify(x => x.Error(It.IsAny<string>()))
                .WasNotCalled();

            logger
                .Verify(x => x.Error(It.IsAny<string>(), It.IsAny<object[]>()))
                .WasNotCalled();

            logger
                .Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .WasNotCalled();
        }

        [Fact]
        public async Task save_async_does_not_fail_if_a_save_callback_fails()
        {
            var sut = new StateServiceBuilder().Build();
            sut.RegisterSaveCallback(_ => Task.Run(() => { throw new Exception("Failed"); }));

            await sut.SaveAsync();
        }

        [Fact]
        public async Task save_async_logs_an_error_if_a_save_callback_fails()
        {
            var logger = new LoggerMock(MockBehavior.Loose);
            var loggerService = new LoggerServiceMock(MockBehavior.Loose);

            loggerService
                .When(x => x.GetLogger(typeof(StateService)))
                .Return(logger);

            var sut = new StateServiceBuilder()
                .WithLoggerService(loggerService)
                .Build();

            sut.RegisterSaveCallback(_ => Task.Run(() => { throw new Exception("whatever"); }));

            await sut.SaveAsync();

            logger
                .Verify(x => x.Error(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .WasCalledExactlyOnce();
        }

        [Fact]
        public void register_save_callback_throws_if_save_task_factory_is_null()
        {
            var sut = new StateServiceBuilder().Build();

            Assert.Throws<ArgumentNullException>(() => sut.RegisterSaveCallback(null));
        }

        [Fact]
        public async Task register_save_callback_returns_a_registration_handle_that_unregisters_the_callback_when_disposed()
        {
            var sut = new StateServiceBuilder().Build();
            var firstExecuted = false;
            var secondExecuted = false;
            var handle = sut.RegisterSaveCallback(_ => Task.Run(() => firstExecuted = true));
            sut.RegisterSaveCallback(_ => Task.Run(() => secondExecuted = true));

            handle.Dispose();

            await sut.SaveAsync();

            Assert.False(firstExecuted);
            Assert.True(secondExecuted);
        }
    }
}