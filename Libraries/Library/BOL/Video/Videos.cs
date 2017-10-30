﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Library.BOL.Video
{
    [Serializable]
    public sealed class Videos : BaseCollection
    {
        #region Generic CollectionBase Code

        #region Properties

        /// <summary>
        /// Indexer Property
        /// </summary>
        /// <param name="Index">Index of object to return</param>
        /// <returns>Video object</returns>
        public Video this[int Index]
        {
            get
            {
                return ((Video)this.InnerList[Index]);
            }

            set
            {
                this.InnerList[Index] = value;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Adds an item to the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Add(Video value)
        {
            return (List.Add(value));
        }

        /// <summary>
        /// Returns the index of an item within the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int IndexOf(Video value)
        {
            return (List.IndexOf(value));
        }

        /// <summary>
        /// Inserts an item into the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void Insert(int index, Video value)
        {
            List.Insert(index, value);
        }


        /// <summary>
        /// Removes an item from the collection
        /// </summary>
        /// <param name="value"></param>
        public void Remove(Video value)
        {
            List.Remove(value);
        }


        /// <summary>
        /// Indicates the existence of an item within the collection
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(Video value)
        {
            // If value is not of type OBJECT_TYPE, this will return false.
            return (List.Contains(value));
        }

        #endregion Public Methods

        #region Private Members

        private const string OBJECT_TYPE = "Library.BOL.Video.Video";
        private const string OBJECT_TYPE_ERROR = "Must be of type Coupon";


        #endregion Private Members

        #region Overridden Methods

        /// <summary>
        /// When Inserting an Item
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        protected override void OnInsert(int index, Object value)
        {
            if (value.GetType() != Type.GetType(OBJECT_TYPE))
                throw new ArgumentException(OBJECT_TYPE_ERROR, "value");
        }


        /// <summary>
        /// When removing an item
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        protected override void OnRemove(int index, Object value)
        {
            if (value.GetType() != Type.GetType(OBJECT_TYPE))
                throw new ArgumentException(OBJECT_TYPE_ERROR, "value");
        }


        /// <summary>
        /// When Setting an Item
        /// </summary>
        /// <param name="index"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected override void OnSet(int index, Object oldValue, Object newValue)
        {
            if (newValue.GetType() != Type.GetType(OBJECT_TYPE))
                throw new ArgumentException(OBJECT_TYPE_ERROR, "newValue");
        }


        /// <summary>
        /// Validates an object
        /// </summary>
        /// <param name="value"></param>
        protected override void OnValidate(Object value)
        {
            if (value.GetType() != Type.GetType(OBJECT_TYPE))
                throw new ArgumentException(OBJECT_TYPE_ERROR);
        }


        #endregion Overridden Methods

        #endregion Generic CollectionBase Code

        #region Static Methods

        /// <summary>
        /// Returns all Video's
        /// </summary>
        /// <returns>Video Collection</returns>
        public static Videos Get()
        {
            return (DAL.FirebirdDB.VideoGet());
        }

        public static Video Get(int ID)
        {
            return (DAL.FirebirdDB.VideoGet(ID));
        }

        public static int GetCount()
        {
            return (DAL.FirebirdDB.VideoCount());
        }

        /// <summary>
        /// Deletes a video
        /// </summary>
        /// <param name="video"></param>
        public static void Delete(Video video)
        {
            DAL.FirebirdDB.VideoDelete(video);
        }

        /// <summary>
        /// Creates a new video object
        /// </summary>
        /// <param name="description"></param>
        /// <param name="video"></param>
        /// <returns></returns>
        public static Video Create(string description, string reference)
        {
            int id = DAL.FirebirdDB.VideoCreate(description, reference);

            Video Result = new Video(id, reference, description);
            return (Result);
        }

        #endregion Static Methods
    }
}