using System;
using System.Collections.Generic;
using System.Linq;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("Balloony")]
[assembly: ExportEffect(typeof(Balloony.Droid.TouchEffect), "TouchEffect")]
namespace Balloony.Droid
{
    public class TouchEffect : PlatformEffect
    {
        Android.Views.View view;
        Element formsElement;
        Balloony.TouchEffect libTouchEffect;
        bool capture;
        Func<double, double> fromPixels;
        int[] twoIntArray = new int[2];

        static Dictionary<Android.Views.View, TouchEffect> viewDictionary =
          new Dictionary<Android.Views.View, TouchEffect>();

        static Dictionary<int, TouchEffect> idToEffectDictionary =
          new Dictionary<int, TouchEffect>();

        protected override void OnAttached()
        {
            // Get the Android View corresponding to the Element that the effect is attached to
            view = Control == null ? Container : Control;

            // Get access to the TouchEffect class in the .NET Standard library
            Balloony.TouchEffect touchEffect =
              (Balloony.TouchEffect)Element.Effects.
                FirstOrDefault(e => e is Balloony.TouchEffect);

            if (touchEffect != null && view != null)
            {
                viewDictionary.Add(view, this);

                formsElement = Element;

                libTouchEffect = touchEffect;

                // Save fromPixels function
                fromPixels = view.Context.FromPixels;

                // Set event handler on View
                view.Touch += OnTouch;
            }
        }

        protected override void OnDetached()
        {
            if (viewDictionary.ContainsKey(view))
            {
                viewDictionary.Remove(view);
                view.Touch -= OnTouch;
            }
        }

        void OnTouch(object sender, Android.Views.View.TouchEventArgs args)
        {
            // Two object common to all the events
            Android.Views.View senderView = sender as Android.Views.View;
            MotionEvent motionEvent = args.Event;

            // Get the pointer index
            int pointerIndex = motionEvent.ActionIndex;

            // Get the id that identifies a finger over the course of its progress
            int id = motionEvent.GetPointerId(pointerIndex);


            senderView.GetLocationOnScreen(twoIntArray);
            Point screenPointerCoords = new Point(twoIntArray[0] + motionEvent.GetX(pointerIndex),
                                twoIntArray[1] + motionEvent.GetY(pointerIndex));


            // Use ActionMasked here rather than Action to reduce the number of possibilities
            switch (args.Event.ActionMasked)
            {
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    FireEvent(this, id, Balloony.TouchEffect.TouchActionType.Pressed, screenPointerCoords, true);

                    idToEffectDictionary.Add(id, this);

                    capture = libTouchEffect.Capture;
                    break;

                case MotionEventActions.Move:
                    // Multiple Move events are bundled, so handle them in a loop
                    for (pointerIndex = 0; pointerIndex < motionEvent.PointerCount; pointerIndex++)
                    {
                        id = motionEvent.GetPointerId(pointerIndex);

                        if (capture)
                        {
                            senderView.GetLocationOnScreen(twoIntArray);

                            screenPointerCoords = new Point(twoIntArray[0] + motionEvent.GetX(pointerIndex),
                                            twoIntArray[1] + motionEvent.GetY(pointerIndex));

                            FireEvent(this, id, Balloony.TouchEffect.TouchActionType.Moved, screenPointerCoords, true);
                        }
                        else
                        {
                            CheckForBoundaryHop(id, screenPointerCoords);

                            if (idToEffectDictionary[id] != null)
                            {
                                FireEvent(idToEffectDictionary[id], id, Balloony.TouchEffect.TouchActionType.Moved, screenPointerCoords, true);
                            }
                        }
                    }
                    break;

                case MotionEventActions.Up:
                case MotionEventActions.Pointer1Up:
                    if (capture)
                    {
                        FireEvent(this, id, Balloony.TouchEffect.TouchActionType.Released, screenPointerCoords, false);
                    }
                    else
                    {
                        CheckForBoundaryHop(id, screenPointerCoords);

                        if (idToEffectDictionary[id] != null)
                        {
                            FireEvent(idToEffectDictionary[id], id, Balloony.TouchEffect.TouchActionType.Released, screenPointerCoords, false);
                        }
                    }
                    idToEffectDictionary.Remove(id);
                    break;

                case MotionEventActions.Cancel:
                    if (capture)
                    {
                        FireEvent(this, id, Balloony.TouchEffect.TouchActionType.Cancelled, screenPointerCoords, false);
                    }
                    else
                    {
                        if (idToEffectDictionary[id] != null)
                        {
                            FireEvent(idToEffectDictionary[id], id, Balloony.TouchEffect.TouchActionType.Cancelled, screenPointerCoords, false);
                        }
                    }
                    idToEffectDictionary.Remove(id);
                    break;
            }
        }

        void CheckForBoundaryHop(int id, Point pointerLocation)
        {
            TouchEffect touchEffectHit = null;

            foreach (Android.Views.View view in viewDictionary.Keys)
            {
                // Get the view rectangle
                try
                {
                    view.GetLocationOnScreen(twoIntArray);
                }
                catch // System.ObjectDisposedException: Cannot access a disposed object.
                {
                    continue;
                }
                Rectangle viewRect = new Rectangle(twoIntArray[0], twoIntArray[1], view.Width, view.Height);

                if (viewRect.Contains(pointerLocation))
                {
                    touchEffectHit = viewDictionary[view];
                }
            }

            if (touchEffectHit != idToEffectDictionary[id])
            {
                if (idToEffectDictionary[id] != null)
                {
                    FireEvent(idToEffectDictionary[id], id, Balloony.TouchEffect.TouchActionType.Exited, pointerLocation, true);
                }
                if (touchEffectHit != null)
                {
                    FireEvent(touchEffectHit, id, Balloony.TouchEffect.TouchActionType.Entered, pointerLocation, true);
                }
                idToEffectDictionary[id] = touchEffectHit;
            }
        }

        void FireEvent(TouchEffect touchEffect, int id, Balloony.TouchEffect.TouchActionType actionType, Point pointerLocation, bool isInContact)
        {
            // Get the method to call for firing events
            Action<Element, Balloony.TouchEffect.TouchActionEventArgs> onTouchAction = touchEffect.libTouchEffect.OnTouchAction;

            // Get the location of the pointer within the view
            touchEffect.view.GetLocationOnScreen(twoIntArray);
            double x = pointerLocation.X - twoIntArray[0];
            double y = pointerLocation.Y - twoIntArray[1];
            Point point = new Point(fromPixels(x), fromPixels(y));

            // Call the method
            onTouchAction(touchEffect.formsElement,
              new Balloony.TouchEffect.TouchActionEventArgs(id, actionType, point, isInContact));
        }
    }
}