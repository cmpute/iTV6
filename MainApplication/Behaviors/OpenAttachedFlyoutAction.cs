using Microsoft.Xaml.Interactivity;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace iTV6.Behaviors
{
    public class OpenAttachedFlyoutAction : DependencyObject, IAction
    {
        public object Execute(object sender, object parameter)
        {
            if (parameter is RightTappedRoutedEventArgs eventArgs)
            {
                if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1))
                {
                    return null;
                }
                OpenMenu(sender, eventArgs.GetPosition(sender as FrameworkElement));
            }
            else
            {
                OpenMenu(sender, null);
            }

            return null;
        }

        public void OpenMenu(object sender, Point? position)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            if (senderElement == null) return;

            var flyout = FlyoutBase.GetAttachedFlyout(senderElement);
            if (position != null && flyout is MenuFlyout)
                (flyout as MenuFlyout).ShowAt(senderElement, position.Value);
            else
                flyout.ShowAt(senderElement);
        }
    }
}