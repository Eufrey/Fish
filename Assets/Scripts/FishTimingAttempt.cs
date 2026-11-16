using System;

namespace Fish
{
    // Plain C# rules keep the attempt lifecycle independent of UI and input devices.
    public sealed class FishTimingAttempt
    {
        public enum AttemptState { Closed, Running, Success, Failure }

        public const float TargetStart = 0.6f;
        public const float TargetEnd = 0.8f;
        public const float IndicatorSpeed = 0.65f;

        public AttemptState State { get; private set; } = AttemptState.Closed;
        public float Position { get; private set; }
        public int ResolvedFrame { get; private set; } = -1;
        private int startedFrame;
        private float travel;
        private bool catchArmed;
        private bool catchClaimed;

        public void Begin(int frame)
        {
            State = AttemptState.Running;
            startedFrame = frame;
            ResolvedFrame = -1;
            Position = 0f;
            travel = 0f;
            catchArmed = false;
            catchClaimed = false;
        }

        public bool Retry(int frame)
        {
            if ((State != AttemptState.Success && State != AttemptState.Failure) || frame <= ResolvedFrame)
                return false;
            Begin(frame);
            return true;
        }

        public void Tick(float deltaSeconds, int frame, bool catchPressed, bool catchHeld)
        {
            if (State != AttemptState.Running || frame <= startedFrame) return;
            if (!catchHeld) catchArmed = true;

            if (catchPressed && catchArmed)
            {
                // Judge the last displayed position before advancing this frame.
                State = IsInsideTarget(Position) ? AttemptState.Success : AttemptState.Failure;
                ResolvedFrame = frame;
                return;
            }

            travel = (travel + Math.Max(0f, deltaSeconds) * IndicatorSpeed) % 2f;
            Position = travel <= 1f ? travel : 2f - travel;
        }

        public void Cancel() => State = AttemptState.Closed;

        public bool TryClaimCatch()
        {
            if (State != AttemptState.Success || catchClaimed) return false;
            catchClaimed = true;
            return true;
        }

        public static bool IsInsideTarget(float position) => position >= TargetStart && position <= TargetEnd;
    }
}
