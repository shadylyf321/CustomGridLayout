using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace UnityEngine.UI
{
    [System.Serializable]
    public class CirecleParams
    {
        public float HorizontalAxis = 300f;//横轴
        public float VerticalAxis = 50f;//纵轴
        public float PrjScale = 0.5f;//z轴透视缩放比
    }
    public class CircleGridLayout : LayoutGroup, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        /// <summary>
        /// 椭圆参数
        /// </summary>
        [SerializeField]
        CirecleParams mCircleParams = new CirecleParams();
        [SerializeField]
        float mMoveDelta = 0f;
        float mPreMoveDelta = 0f;
        float mAngleVel = 0;//角速度
        int mdir = 1;//旋转方向
        List<RectTransform> mSortZChilds = new List<RectTransform>();
        bool mDragging = false;

        int SortZ(RectTransform r1, RectTransform r2)
        {
            return r1.transform.position.z > r2.transform.position.z ? 1 : -1;
        }

        int SortIndex(RectTransform r1, RectTransform r2)
        {
            return  r1.name.CompareTo(r2.name);
        }

        public void OnDrag(PointerEventData eventData)
        {
            mPreMoveDelta = mMoveDelta;
            mMoveDelta += eventData.delta.x / 20f;
            SetDirty();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            mDragging = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            mDragging = false;
        }
        public override void CalculateLayoutInputHorizontal()
        {
            //获得节点下的rectTransform
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

        void  CalculateHoriSize()
        {
            var width = mCircleParams.HorizontalAxis * 2 * transform.localScale.x;
            SetLayoutInputForAxis(width, width, width, 0);
        }

        void CalculateVertiSize()
        {
            var height = mCircleParams.VerticalAxis * 2 * transform.localScale.y;
            SetLayoutInputForAxis(height, height, height, 1);
        }

        public override void SetLayoutHorizontal()
        {
            var count = rectChildren.Count;
            for (int i = 0; i < mSortZChilds.Count; ++i)
            {
                mSortZChilds[i].transform.position = GetChildPosOnCircle(i, mSortZChilds.Count);
                mSortZChilds[i].transform.localScale =
                    Vector3.Lerp(Vector3.one, Vector3.one  * mCircleParams.PrjScale,
                        (mSortZChilds[i].transform.position.y - transform.position.y)
                            / mCircleParams.VerticalAxis * 2);
            }
            mSortZChilds.Sort(SortZ);
            for (int i = 0; i < mSortZChilds.Count; ++i)
                mSortZChilds[i].transform.SetSiblingIndex(i);
        }

        public override void SetLayoutVertical()
        {

        }

        Vector3 GetChildPosOnCircle(int index, int Count)
        {
            if (Count == 0)
                return Vector3.zero;
            var pos = transform.position + new Vector3(0, mCircleParams.VerticalAxis * transform.localScale.y, 0);
            var a = mCircleParams.HorizontalAxis * transform.localScale.x;
            var b = mCircleParams.VerticalAxis * transform.localScale.y;
            var angle = (360f * index / Count  - 90f) / 180f * Mathf.PI + mMoveDelta / 180f * Mathf.PI;
            var x0 = a * Mathf.Cos(angle);
            var y0 = b * Mathf.Sin(angle);
            var zo = Mathf.Lerp(1, -1, (y0 + b) / (2 * b));
            return new Vector3(pos.x + x0, pos.y + y0, pos.z + zo);
        }

        void LateUpdate()
        {
            if (!mDragging && Mathf.Abs(mAngleVel - 0) > 0.01f)
            {
                mMoveDelta += mAngleVel * Time.unscaledDeltaTime;
                mAngleVel -= mAngleVel * Time.unscaledDeltaTime;
                if (mdir > 0 && mAngleVel <= 0.1f ||
                    mdir < 0 && mAngleVel >= -0.1f)
                    mAngleVel = 0;
                SetDirty();
            }

            if (mDragging)
            {
                mAngleVel = (mMoveDelta - mPreMoveDelta) / Time.unscaledDeltaTime;
                mdir = mAngleVel < 0 ? -1 : 1;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            var pos = transform.position + new Vector3(0, mCircleParams.VerticalAxis * transform.localScale.y,0);
            var a = mCircleParams.HorizontalAxis * transform.localScale.x;
            var b = mCircleParams.VerticalAxis * transform.localScale.y;
            var step = 30;
            for (int i = 0; i < step; ++i)
            {
                var angle = 360f / step;
                angle = angle / 180f * Mathf.PI;
                var x0 = a * Mathf.Cos(angle * i);
                var y0 = b * Mathf.Sin(angle * i);
                var x1 = a * Mathf.Cos(angle * (i + 1));
                var y1 = b * Mathf.Sin(angle * (i + 1));
                var next = new Vector3(pos.x + x1, pos.y + y1, pos.z);
                var cur = new Vector3(pos.x + x0, pos.y + y0, pos.z);
                UnityEditor.Handles.DrawLine(cur, next);
            }
        }
#endif
    }
}