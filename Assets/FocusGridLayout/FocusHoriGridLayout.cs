using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
     public class FocusHoriGridLayout : LayoutGroup
    {
        [SerializeField]
        [Range(2, 10)]
        int mFocusIndex = 2;
        float mFocusValue = 2;
        [SerializeField]
        [Range(1, 20)]
        int mFoucsRange = 2;
        [SerializeField]
        float mSpacing = 0;
        [SerializeField]
        [Range(1f, 1.5f)]
        float mFScale = 1.2f;

        List<RectTransform> mSortZChilds = new List<RectTransform>();

        int SortZ(RectTransform r1, RectTransform r2)
        {
            return r1.transform.position.z > r2.transform.position.z ? 1 : -1;
        }

        int SortIndex(RectTransform r1, RectTransform r2)
        {
            return r1.name.CompareTo(r2.name);
        }

        protected override void Awake()
        {
            base.Awake();
            mFocusValue = mFocusIndex;
        }

        /// <summary>
        /// 获取关注点x坐标
        /// </summary>
        float GetFocusPosition()
        {
            return (rectChildren[0].sizeDelta[0] + mSpacing) * (mFocusValue - 1); 
        }

        /// <summary>
        ///获取关注(放大)区间 
        /// </summary>
        /// <returns>item1:左 item2:右</returns>
        ValueTuple<float, float> GetFocusRange()
        {
            var focusP = GetFocusPosition();
            return (focusP - mFoucsRange * rectChildren[0].sizeDelta[0] - mFoucsRange * mSpacing,
                        focusP + mFoucsRange * rectChildren[0].sizeDelta[0] + mFoucsRange * mSpacing);
        }
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            mSortZChilds.Clear();
            mSortZChilds.AddRange(rectChildren);
            mSortZChilds.Sort(SortIndex);
            CalculateHoriSize();
        }

        public override void CalculateLayoutInputVertical()
        {
            CalculateVertiSize();
        }

        void CalculateHoriSize()
        {
            float width = 0;
            if (rectChildren.Count > 0)
                width = rectChildren.Count * rectChildren[0].sizeDelta[0] +
                        (rectChildren.Count - 1) * mSpacing;
            SetLayoutInputForAxis(width, width, width, 0);
        }

        void CalculateVertiSize()
        {
            float height = 0;
            if (rectChildren.Count > 0)
                height = rectChildren.Count * rectChildren[0].sizeDelta[1];
            SetLayoutInputForAxis(height, height, height, 1);
        }

        public override void SetLayoutHorizontal()
        {
            if (mSortZChilds.Count == 0)
                return;
            var range = GetFocusRange();
            var focuse = GetFocusPosition();
            var size = mSortZChilds[0].sizeDelta[0];
            for (int i =0; i < mSortZChilds.Count; ++i)
            {
                var pos = i * size + i * mSpacing;
                var dist2Focus = Mathf.Abs(pos - focuse);
                var t = dist2Focus / ((range.Item2 - range.Item1) * 0.5f);
                var scale = Mathf.Lerp(mFScale, 1, t);
                mSortZChilds[i].SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, pos, size);
                mSortZChilds[i].transform.localScale = new Vector3(scale, scale, scale);
                //focus最后绘制,z用来做排序
                mSortZChilds[i].transform.localPosition = new Vector3(mSortZChilds[i].transform.localPosition.x,
                    mSortZChilds[i].transform.localPosition.y, -t);
            }
            mSortZChilds.Sort(SortZ);
            for (int i = 0; i < mSortZChilds.Count; ++i)
                mSortZChilds[i].transform.SetSiblingIndex(i);
        }

        public override void SetLayoutVertical() { }

        public void OnScrollValueChange(Vector2 value)
        {
            if (rectChildren.Count == 0)
                return;
            mFocusValue = mFocusIndex - rectTransform.anchoredPosition.x  / rectChildren[0].sizeDelta.x;
            SetDirty();
        }

        protected override void OnValidate()
        {
            mFocusValue = mFocusIndex;
            base.OnValidate();
        }
    }
}
