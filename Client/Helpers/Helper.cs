using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoinPoker
{

    public class Helper
    {
        /// <summary>
        /// Gets the list of routed event handlers subscribed to the specified routed event.
        /// </summary>
        /// <param name="element">The UI element on which the event is defined.</param>
        /// <param name="routedEvent">The routed event for which to retrieve the event handlers.</param>
        /// <returns>The list of subscribed routed event handlers.</returns>
        public static RoutedEventHandlerInfo[] GetRoutedEventHandlers(UIElement element, RoutedEvent routedEvent)
        {
            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

            // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
            // for getting an array of the subscribed event handlers.
            try
            {
                var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
                    "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
                    eventHandlersStore, new object[] { routedEvent });

                return routedEventHandlers;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static Point MovePointTowardsByScale(Point a, Point b, double distance, double scale)
        {
            var vector = new Point(b.X - a.X, b.Y - a.Y);
            var length = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y) / scale;
            var unitVector = new Point(vector.X / length, vector.Y / length);
            return new Point(a.X + unitVector.X * distance, a.Y + unitVector.Y * distance);
        }

        public static Point MovePointTowards(Point a, Point b, double distance, double scaleX = 1.0, double scaleY = 1.0)
        {
            var vector = new Point(b.X - a.X, b.Y - a.Y);
            var length = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
            var unitVector = new Point(vector.X / length, vector.Y / length);
            return new Point(a.X + unitVector.X * distance * scaleX, a.Y + unitVector.Y * distance * scaleY);
        }

        public static T FindVisualParent<T>(DependencyObject sender) where T : DependencyObject
        {
            if (sender == null)
            {
                return (null);
            }
            else if (VisualTreeHelper.GetParent(sender) is T)
            {
                return (VisualTreeHelper.GetParent(sender) as T);
            }
            else
            {
                DependencyObject parent = VisualTreeHelper.GetParent(sender);
                return (FindVisualParent<T>(parent));
            }
        } 

        public static byte[] BufferFromImage(BitmapImage imageSource)
        {
            Stream stream = imageSource.StreamSource;
            byte[] buffer = null;

            if (stream != null && stream.Length > 0)
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    buffer = br.ReadBytes((Int32)stream.Length);
                }
            }

            return buffer;
        }

        public static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0) return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static void RemoveClickEvent(Button b)
        {
            var routedEventHandlers = GetRoutedEventHandlers(b, ButtonBase.ClickEvent);
            foreach (var routedEventHandler in routedEventHandlers)
                b.Click -= (RoutedEventHandler)routedEventHandler.Handler;
        }
    }
}
