using System;
namespace Gawa.ACRender
{
    /// <summary>
    /// Should be implemented by objects that can be rendered directly by the
    /// engine.
    /// </summary>
    interface IRenderable
    {
        /// <summary>
        /// Dispose the object's rendering data, because the device has
        /// been lost.
        /// </summary>
        void DisposeDueToLostDevice();
        /// <summary>
        /// (Re) initialize the objct for rendering
        /// </summary>
        void ReInitialize();
        /// <summary>
        /// Render the object to the scene
        /// </summary>
        void Render();
    }
}
