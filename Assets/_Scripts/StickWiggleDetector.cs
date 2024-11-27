using UnityEngine;

namespace _Scripts
{
    public class StickWiggleDetector
    {
        private enum WiggleState
        {
            Neutral,
            MovedLeft,
            MovedRight
        }

        private WiggleState state = WiggleState.Neutral;
        private int wiggleCount = 0;

        private readonly float threshold = 0.25f; // Adjust as needed
        private float timeSinceLastWiggle = 0f;

        public int WiggleCount => wiggleCount;

        public void Update(float horizontalInput, float deltaTime)
        {
            Debug.Log("Stick Wiggle Update Entered. Count: " + wiggleCount + " Input: " + horizontalInput);
            timeSinceLastWiggle += deltaTime;

            if (Mathf.Abs(horizontalInput) > threshold)
            {
                if (state == WiggleState.Neutral)
                {
                    wiggleCount++;
                    state = horizontalInput > 0 ? WiggleState.MovedRight : WiggleState.MovedLeft;
                }
                else if (state == WiggleState.MovedLeft && horizontalInput > threshold)
                {
                    // From left to right
                    wiggleCount++;
                    timeSinceLastWiggle = 0f;
                    state = WiggleState.MovedRight;
                }
                else if (state == WiggleState.MovedRight && horizontalInput < -threshold)
                {
                    // From right to left
                    wiggleCount++;
                    timeSinceLastWiggle = 0f;
                    state = WiggleState.MovedLeft;
                }
            }
            else
            {
                // Stick near center
                state = WiggleState.Neutral;
            }
            Debug.Log("Stick Wiggle Update Left. Count: " + wiggleCount + " Input: " + horizontalInput);
        }

        public void Reset()
        {
            wiggleCount = 0;
            state = WiggleState.Neutral;
            timeSinceLastWiggle = 0f;
        } 
    }
}