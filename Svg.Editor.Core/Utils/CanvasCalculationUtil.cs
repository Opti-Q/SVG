namespace Svg.Core.Utils
{
    public static class CanvasCalculationUtil
    {
        public static float GetRelativeDimension(float canvasDimension, float targetDimension, float zoomFactor = 1.0f)
        {
            var halfRelStartWidth = targetDimension / zoomFactor;
            return -canvasDimension + halfRelStartWidth;
        }
    }
}
