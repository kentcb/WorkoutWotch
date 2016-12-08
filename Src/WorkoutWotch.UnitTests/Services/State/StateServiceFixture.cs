namespace WorkoutWotch.UnitTests.Services.State
{
    using System;
    using System.Reactive;
    using System.Reactive.Linq;
    using Builders;
    using PCLMock;
    using WorkoutWotch.UnitTests.Services.State.Mocks;
    using Xunit;

    public sealed class StateServiceFixture
    {
        [Fact]
        public void get_forwards_the_call_onto_the_blob_cache()
        {
            var blobCache = new BlobCacheMock();

            blobCache
                .When(x => x.Get(It.IsAny<string>()))
                .Return(Observable.Return(new byte[0]));

            var sut = new StateServiceBuilder()
                .WithBlobCache(blobCache)
                .Build();

            sut.Get<string>("some key");

            // we don't verify the specific key because Akavache does some key manipulation internally
            blobCache
                .Verify(x => x.Get(It.IsAny<string>()))
                .WasCalledExactlyOnce();
        }

        [Fact]
        public void set_forwards_the_call_onto_the_blob_cache()
        {
            var blobCache = new BlobCacheMock(MockBehavior.Loose);

            blobCache
                .When(x => x.Insert(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DateTimeOffset?>()))
                .Return(Observables.Unit);

            var sut = new StateServiceBuilder()
                .WithBlobCache(blobCache)
                .Build();

            sut.Set<string>("some key", "some value");

            // we don't verify the specific key because Akavache does some key manipulation internally
            blobCache
                .Verify(x => x.Insert(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DateTimeOffset?>()))
                .WasCalledExactlyOnce();
        }

        [Fact]
        public void remove_forwards_the_call_onto_the_blob_cache()
        {
            var blobCache = new BlobCacheMock(MockBehavior.Loose);

            blobCache
                .When(x => x.Invalidate(It.IsAny<string>()))
                .Return(Observables.Unit);

            var sut = new StateServiceBuilder()
                .WithBlobCache(blobCache)
                .Build();

            sut.Remove<string>("some key");

            // we don't verify the specific key because Akavache does some key manipulation internally
            blobCache
                .Verify(x => x.Invalidate(It.IsAny<string>()))
                .WasCalledExactlyOnce();
        }

        [Fact]
        public void save_executes_all_tasks_returned_by_saved_callbacks()
        {
            var sut = new StateServiceBuilder()
                .Build();
            var firstExecuted = false;
            var secondExecuted = false;
            sut
                .RegisterSaveCallback(
                    _ =>
                    {
                        firstExecuted = true;
                        return Observables.Unit;
                    });
            sut
                .RegisterSaveCallback(
                    _ =>
                    {
                        secondExecuted = true;
                        return Observables.Unit;
                    });

            sut.Save().Subscribe();

            Assert.True(firstExecuted);
            Assert.True(secondExecuted);
        }

        [Fact]
        public void save_ignores_any_null_tasks_returned_by_saved_callbacks()
        {
            var sut = new StateServiceBuilder()
                .Build();

            var firstExecuted = false;
            var secondExecuted = false;
            sut
                .RegisterSaveCallback(
                    _ =>
                    {
                        firstExecuted = true;
                        return Observables.Unit;
                    });
            sut.RegisterSaveCallback(_ => null);
            sut
                .RegisterSaveCallback(
                    _ =>
                    {
                        secondExecuted = true;
                        return Observables.Unit;
                    });

            sut.Save().Subscribe();

            Assert.True(firstExecuted);
            Assert.True(secondExecuted);
        }

        [Fact]
        public void save_does_not_fail_if_a_save_callback_fails()
        {
            var sut = new StateServiceBuilder()
                .Build();
            sut.RegisterSaveCallback(_ => Observable.Throw<Unit>(new Exception("Failed")));

            sut.Save().Subscribe();
        }

        [Fact]
        public void save_completes_even_if_there_are_no_save_callbacks()
        {
            var sut = new StateServiceBuilder()
                .Build();

            var completed = false;
            sut
                .Save()
                .Subscribe(_ => completed = true);

            Assert.True(completed);
        }

        [Fact]
        public void register_save_callback_returns_a_registration_handle_that_unregisters_the_callback_when_disposed()
        {
            var sut = new StateServiceBuilder()
                .Build();
            var firstExecuted = false;
            var secondExecuted = false;

            var handle = sut
                .RegisterSaveCallback(
                    _ =>
                    {
                        firstExecuted = true;
                        return Observables.Unit;
                    });
            sut
                .RegisterSaveCallback(
                    _ =>
                    {
                        secondExecuted = true;
                        return Observables.Unit;
                    });

            handle.Dispose();

            sut.Save().Subscribe();

            Assert.False(firstExecuted);
            Assert.True(secondExecuted);
        }
    }
}