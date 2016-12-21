using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace Gawa.ACRender
{
    public class Camera
    {
        public Camera(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
        }

        private Vector3 position = new Vector3(2, 0, 2);    // These are the most important values in
        private Vector3 target = new Vector3(0, 0, 0);      // the camera class, and they are what
        private Vector3 upVector = new Vector3(0, 1, 0);    // we make our final View matrix with

        private int screenWidth;
        private int screenHeight;

        // These values tell the camera how wide our view angle is and how close/far we can see
        private float nearClip = 0.1f;
        private float farClip = 100.0f;
        private float fov = (float)Math.PI / 4.0f;

        // We will use these in the math processes that control movement later
        private float hRotation = (float)Math.PI / 2.0f;
        private float vRotation = 0.0f;
        private float radius = 1.0f;

        // Public matrix properties that our device can use to set its own matrices before rendering
        public Matrix World { get { return Matrix.Identity; } }
        public Matrix View { get { return Matrix.LookAtLH(position, target, upVector); } }
        public Matrix Projection
        {
            get
            {
                return Matrix.PerspectiveFovLH
                (
                    fov,
                    (float)screenWidth / (float)screenHeight,
                    nearClip, farClip
                );
            }
        }

        public void UpdatePosition()
        {
            // (radius * Math.Cos(vRotation)) is the temporary radius after the y component shift
            position.X = (float)(radius * Math.Cos(vRotation) * Math.Cos(hRotation));
            position.Y = (float)(radius * Math.Sin(vRotation));
            position.Z = (float)(radius * Math.Cos(vRotation) * Math.Sin(hRotation));

            // Keep all rotations between 0 and 2PI
            hRotation = hRotation > (float)Math.PI * 2 ? hRotation - (float)Math.PI * 2 : hRotation;
            hRotation = hRotation < 0 ? hRotation + (float)Math.PI * 2 : hRotation;

            vRotation = vRotation > (float)Math.PI * 2 ? vRotation - (float)Math.PI * 2 : vRotation;
            vRotation = vRotation < 0 ? vRotation + (float)Math.PI * 2 : vRotation;

            // Switch up-vector based on vertical rotation
            upVector = vRotation > Math.PI / 2 && vRotation < Math.PI / 2 * 3 ?
                new Vector3(0, -1, 0) : new Vector3(0, 1, 0);

            // Translate these coordinates by the target objects spacial location
            position += target;
        }

        public void Rotate(float h, float v)
        {
            hRotation += h;
            vRotation += v;

            // We will do this after each type of camera movement
            UpdatePosition();
        }

        public void Zoom(float dist)
        {
            radius += dist;
            if (radius < .01f) radius = .01f;

            UpdatePosition();
        }
    }
}