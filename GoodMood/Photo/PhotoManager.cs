﻿/*
 * Copyright 2015 Andrea Del Signore sejerpz@gmail.com
 */

using GoodMood.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoodMood.Photo
{
    class PhotoManager
    {
        private Image image;
        private PhotoUri pictureUri;
        private string lastDowloadedUri = null;
        private System.Threading.Timer timer;
        private bool isRunning = false;

        public event EventHandler PictureUpdateBegin;
        public event EventHandler PictureUpdateEnd;
        public event EventHandler<ThreadExceptionEventArgs> PictureUpdateError;
        public event EventHandler PictureUpdateSuccess;

        public PhotoUri Uri
        {
            get
            {
                return pictureUri;
            }
            set
            {
                pictureUri = value;
            }
        }

        public Image Image
        {
            get
            {
                return image;
            }
            private set
            {
                image = value;
            }
        }
        
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }

        public PhotoManager(PhotoUri pictureUri)
        {
            this.pictureUri = pictureUri;
        }

        private async void CheckUpdate(object state)
        {
            int nextCheckInterval = 1 * 60 * 60 * 1000;  // recheck in 1 hour
            var updated = await Update();

            if (updated)
            {
                var now = DateTime.Now;
                var nextTick = now.Date.AddDays(1).Date.Subtract(now);
                nextCheckInterval = 10000 + (int)(nextTick.TotalSeconds) * 1000; // 10 sec. after midnight
            }
            timer.Change(nextCheckInterval, Timeout.Infinite);
        }

        protected virtual void OnPictureUpdateBegin(EventArgs e)
        {
            if (PictureUpdateBegin != null)
                PictureUpdateBegin(this, e);
        }

        protected virtual void OnPictureUpdateEnd(EventArgs e)
        {
 	        if (PictureUpdateEnd != null)
                PictureUpdateEnd(this, e);
        }

        protected virtual void OnPictureUpdateError(ThreadExceptionEventArgs e)
        {
            if (PictureUpdateError != null)
                PictureUpdateError(this, e);
        }

        protected virtual void OnPictureUpdateSuccess(EventArgs e)
        {
            if (PictureUpdateSuccess != null)
                PictureUpdateSuccess(this, e);
        }

        public void Start()
        {
            timer = new System.Threading.Timer(this.CheckUpdate, this, 5 * 1000, Timeout.Infinite);
            isRunning = true;
        }

        public void Stop()
        {
            if (isRunning)
            {
                timer.Change(Timeout.Infinite, Timeout.Infinite);
                timer.Dispose();
                isRunning = false;
                timer = null;
            }
        }

        public async Task<bool> Update()
        {
            lastDowloadedUri = null; // invalidate cached uri
            return await CheckAndDowloadImage();
        }

        private async Task<bool> CheckAndDowloadImage()
        {
            bool updated = false;
            Image newImage;

            OnPictureUpdateBegin(EventArgs.Empty);
            try
            {
                await this.pictureUri.Update();
                if (!string.IsNullOrEmpty(this.pictureUri.PhotoAddress))
                {
                    newImage = await DownloadImage(this.pictureUri);
                }
                else
                {
                    newImage = null;
                }

                updated = newImage != this.Image;
                this.Image = newImage;
                OnPictureUpdateSuccess(EventArgs.Empty);
                return updated;
            }
            catch(Exception ex)
            {
                OnPictureUpdateError(new ThreadExceptionEventArgs(ex));
                return false;
            }
            finally
            {
                OnPictureUpdateEnd(EventArgs.Empty);
            }
        }

        private async Task<Image> DownloadImage(PhotoUri pictureUri)
        {
            try
            {
                Image newImage = null;

                // check cached uri
                if (string.IsNullOrEmpty(lastDowloadedUri) || pictureUri.PhotoAddress != lastDowloadedUri)
                {
                    var dowloader = new PhotoDowloader();

                    await dowloader.Download(pictureUri);
                    lastDowloadedUri = pictureUri.PhotoAddress;
                    newImage = dowloader.Image;
                }
                else
                    newImage = this.image; // return previous downloaded image

                return newImage;
            }
            catch
            {
                throw;
            }
        }
    }
}
