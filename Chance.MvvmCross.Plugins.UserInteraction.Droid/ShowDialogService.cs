using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid.Platform;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace Chance.MvvmCross.Plugins.UserInteraction.Droid
{
    public class ShowDialogService : IShowDialogService
    {
        public Task<ConfirmThreeButtonsResponse> ShowAsync(string message, string title = null, View view = null, string positive = null, string negative = null, string neutral = null, CancellationToken ct = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<ConfirmThreeButtonsResponse>();

            Application.SynchronizationContext.Post(ignored =>
            {
                if (CurrentActivity == null)
                    tcs.TrySetCanceled();
                else
                {
                    var builder = this.CreateBuilder()
                        .SetMessage(message)
                        .SetTitle(title);

                    if (view != null)
                        builder = builder.SetView(view);

                    if (positive != null)
                        builder = builder.SetPositiveButton(positive, (s, e) => tcs.TrySetResult(ConfirmThreeButtonsResponse.Positive));

                    if (negative != null)
                        builder = builder.SetNegativeButton(negative, (s, e) => tcs.TrySetResult(ConfirmThreeButtonsResponse.Negative));

                    if (neutral != null)
                        builder = builder.SetNeutralButton(neutral, (s, e) => tcs.TrySetResult(ConfirmThreeButtonsResponse.Neutral));

                    var dialog = builder
                        .Create();

                    if (ct.CanBeCanceled)
                    {
                        var subscription = ct.Register(() =>
                        {
                            Application.SynchronizationContext.Post(ignored2 => dialog.SafeDismiss(), null);
                            tcs.TrySetResult(ConfirmThreeButtonsResponse.Negative);
                        });

                        // ReSharper disable once MethodSupportsCancellation
                        tcs.Task.ContinueWith(_ => subscription.Dispose());
                    }

                    dialog.Show();
                }
            }, null);

            return tcs.Task;
        }

        protected virtual AlertDialog.Builder CreateBuilder()
        {
            return new AlertDialog.Builder(this.CurrentActivity);
        }

        public Activity CurrentActivity => Mvx.Resolve<IMvxAndroidCurrentTopActivity>().Activity;
    }
}