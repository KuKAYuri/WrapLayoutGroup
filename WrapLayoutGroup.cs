using System;
using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class WrapLayoutGroup : LayoutGroup
    {
        public enum Constraint
        {
            FixedWidth = 0,
            FixedHeight = 1
        }

        [SerializeField] protected Vector2 m_Spacing = Vector2.zero;

        public Vector2 spacing
        {
            get { return m_Spacing; }
            set { SetProperty(ref m_Spacing, value); }
        }

        [SerializeField] protected Constraint m_Constraint = Constraint.FixedWidth;

        public Constraint constraint
        {
            get { return m_Constraint; }
            set { SetProperty(ref m_Constraint, value); }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            constraint = constraint;
        }
#endif

/*--------------------------------------------------------------------------------------------------------------------*/
#if DEVELOP_TEST
        [Serializable]
#endif
        private class Axis
        {
            public int index;
            public bool enable;
            public Rect rect;
            public List<RectTransform> children;

            public Axis(int index)
            {
                this.index = index;
                enable = false;
                rect = Rect.zero;
                children = new List<RectTransform>();
            }

            public override string ToString()
            {
                return $"i:{index} enable:{enable} rect:{rect}";
            }

            public void Enable()
            {
                enable = true;
                rect.Set(0, 0, 0, 0);
                children.Clear();
            }

            public void Disable()
            {
                enable = false;
            }

            public void SetChildrenAlongAxis(Action<RectTransform, int, float> setChildAlongAxis,
                Constraint constraint, TextAnchor childAlignment, Vector2 spacing)
            {
                if (children == null || children.Count == 0)
                {
                    return;
                }
                float curChildPosX = 0f;
                float curChildPosY = 0f;
                RectTransform preChild = children[0];
                for (int i = 0; i < children.Count; i++)
                {
                    RectTransform curChild = children[i];
                    if (i > 1)
                    {
                        preChild = children[i - 1];
                    }
                    if (constraint == Constraint.FixedWidth)
                    {
                        if ((int) childAlignment / 3 == 0)
                        {
                            curChildPosY = rect.y;
                        }
                        else if ((int) childAlignment / 3 == 1)
                        {
                            curChildPosY = rect.center.y - curChild.sizeDelta.y / 2f;
                        }
                        else if ((int) childAlignment / 3 == 2)
                        {
                            curChildPosY = rect.y + rect.height - curChild.sizeDelta.y;
                        }

                        if (preChild.Equals(curChild))
                        {
                            curChildPosX = rect.x;
                        }
                        else
                        {
                            curChildPosX = curChildPosX + preChild.sizeDelta.x + spacing.x;
                        }
                    }
                    else
                    {
                        if ((int) childAlignment % 3 == 0)
                        {
                            curChildPosX = rect.x;
                        }
                        else if ((int) childAlignment % 3 == 1)
                        {
                            curChildPosX = rect.center.x - curChild.sizeDelta.x / 2f;
                        }
                        else if ((int) childAlignment % 3 == 2)
                        {
                            curChildPosX = rect.x + rect.width - curChild.sizeDelta.x;
                        }

                        if (preChild.Equals(curChild))
                        {
                            curChildPosY = rect.y;
                        }
                        else
                        {
                            curChildPosY = curChildPosY + preChild.sizeDelta.y + spacing.y;
                        }
                    }
                    setChildAlongAxis(curChild, 0, curChildPosX);
                    setChildAlongAxis(curChild, 1, curChildPosY);
                }
            }
        }

#if DEVELOP_TEST
        [SerializeField]
#endif
        private Rect _contentRect = Rect.zero;
#if DEVELOP_TEST
        [SerializeField]
#endif
        private List<Axis> _axes = new List<Axis>();

        protected WrapLayoutGroup()
        {
        }

        private void CalculateAxes()
        {
            if (_axes.Count == 0)
            {
                _axes.Add(new Axis(0));
            }
            Axis curAxis = _axes[0];
            curAxis.Enable();

            for (int i = 0; i < rectChildren.Count; i++)
            {
#if DEVELOP_TEST
                if (rectChildren[i].GetComponentInChildren<Text>())
                {
                    rectChildren[i].GetComponentInChildren<Text>().text = i.ToString();
                }
#endif
                RectTransform rectChild = rectChildren[i];
                if (constraint == Constraint.FixedWidth)
                {
                    curAxis.rect.width += rectChild.sizeDelta.x;
                    //如果大于最大长度
                    if (curAxis.rect.width > _contentRect.width)
                    {
                        //如果元素集合为空
                        if (curAxis.children.Count == 0)
                        {
                            //该元素可以加入该轴
                            curAxis.children.Add(rectChild);
                            //设置另一个轴的大小值为这个元素的值
                            curAxis.rect.height = rectChild.sizeDelta.y;
                            if (rectChildren[rectChildren.Count - 1] == rectChild) continue;
                            if (curAxis.index == _axes.Count - 1)
                            {
                                _axes.Add(new Axis(curAxis.index + 1));
                            }
                            curAxis = _axes[curAxis.index + 1];
                            curAxis.Enable();
                        }
                        //如果元素集合不为空
                        else
                        {
                            //该轴长度应该舍去上一个间隔距离加这个元素的长度
                            curAxis.rect.width -= spacing.x + rectChild.sizeDelta.x;
                            //该元素不能加入该轴，应该新建一个轴来容纳这个元素
                            if (curAxis.index == _axes.Count - 1)
                            {
                                _axes.Add(new Axis(curAxis.index + 1));
                            }
                            curAxis = _axes[curAxis.index + 1];
                            curAxis.Enable();
                            i--;
                        }
                    }
                    //如果没有大于最大长度
                    else
                    {
                        //这个元素肯定能加入该轴
                        curAxis.children.Add(rectChild);

                        //再加间隔距离
                        curAxis.rect.width += spacing.x;
                        //如果大于最大长度
                        if (curAxis.rect.width > _contentRect.width)
                        {
                            //该轴长度应该舍去这个间隔距离
                            curAxis.rect.width -= spacing.x;

                            //如果元素集合为空
                            if (curAxis.children.Count == 0)
                            {
                                //设置另一个轴的大小值为这个元素的值
                                curAxis.rect.height = rectChild.sizeDelta.y;
                                if (rectChildren[rectChildren.Count - 1] == rectChild) continue;
                                if (curAxis.index == _axes.Count - 1)
                                {
                                    _axes.Add(new Axis(curAxis.index + 1));
                                }
                                curAxis = _axes[curAxis.index + 1];
                                curAxis.Enable();
                            }
                            //如果元素集合不为空
                            else
                            {
                                //比较设置另一个轴的大小值
                                if (rectChild.sizeDelta.y > curAxis.rect.height)
                                {
                                    curAxis.rect.height = rectChild.sizeDelta.y;
                                }
                                if (rectChildren[rectChildren.Count - 1] == rectChild) continue;
                                if (curAxis.index == _axes.Count - 1)
                                {
                                    _axes.Add(new Axis(curAxis.index + 1));
                                }
                                curAxis = _axes[curAxis.index + 1];
                                curAxis.Enable();
                            }
                        }
                        //如果没有大于最大长度
                        else
                        {
                            if (rectChildren[rectChildren.Count - 1] == rectChild)
                            {
                                curAxis.rect.width -= spacing.x;
                            }
                            //比较设置另一个轴的大小值
                            if (rectChild.sizeDelta.y > curAxis.rect.height)
                            {
                                curAxis.rect.height = rectChild.sizeDelta.y;
                            }
                        }
                    }
                }
                else
                {
                    curAxis.rect.height += rectChild.sizeDelta.y;
                    //如果大于最大长度
                    if (curAxis.rect.height > _contentRect.height)
                    {
                        //如果元素集合为空
                        if (curAxis.children.Count == 0)
                        {
                            //该元素可以加入该轴
                            curAxis.children.Add(rectChild);
                            //设置另一个轴的大小值为这个元素的值
                            curAxis.rect.width = rectChild.sizeDelta.x;
                            if (rectChildren[rectChildren.Count - 1] == rectChild) continue;
                            if (curAxis.index == _axes.Count - 1)
                            {
                                _axes.Add(new Axis(curAxis.index + 1));
                            }
                            curAxis = _axes[curAxis.index + 1];
                            curAxis.Enable();
                        }
                        //如果元素集合不为空
                        else
                        {
                            //该轴长度应该舍去上一个间隔距离加这个元素的长度
                            curAxis.rect.height -= spacing.y + rectChild.sizeDelta.y;
                            //该元素不能加入该轴，应该新建一个轴来容纳这个元素
                            if (curAxis.index == _axes.Count - 1)
                            {
                                _axes.Add(new Axis(curAxis.index + 1));
                            }
                            curAxis = _axes[curAxis.index + 1];
                            curAxis.Enable();
                            i--;
                        }
                    }
                    //如果没有大于最大长度
                    else
                    {
                        //这个元素肯定能加入该轴
                        curAxis.children.Add(rectChild);

                        //再加间隔距离
                        curAxis.rect.height += spacing.y;
                        //如果大于最大长度
                        if (curAxis.rect.height > _contentRect.height)
                        {
                            //该轴长度应该舍去这个间隔距离
                            curAxis.rect.height -= spacing.y;

                            //如果元素集合为空
                            if (curAxis.children.Count == 0)
                            {
                                //设置另一个轴的大小值为这个元素的值
                                curAxis.rect.width = rectChild.sizeDelta.x;
                                if (rectChildren[rectChildren.Count - 1] == rectChild) continue;
                                if (curAxis.index == _axes.Count - 1)
                                {
                                    _axes.Add(new Axis(curAxis.index + 1));
                                }
                                curAxis = _axes[curAxis.index + 1];
                                curAxis.Enable();
                            }
                            //如果元素集合不为空
                            else
                            {
                                //比较设置另一个轴的大小值
                                if (rectChild.sizeDelta.x > curAxis.rect.width)
                                {
                                    curAxis.rect.width = rectChild.sizeDelta.x;
                                }
                                if (rectChildren[rectChildren.Count - 1] == rectChild) continue;
                                if (curAxis.index == _axes.Count - 1)
                                {
                                    _axes.Add(new Axis(curAxis.index + 1));
                                }
                                curAxis = _axes[curAxis.index + 1];
                                curAxis.Enable();
                            }
                        }
                        //如果没有大于最大长度
                        else
                        {
                            if (rectChildren[rectChildren.Count - 1] == rectChild)
                            {
                                curAxis.rect.height -= spacing.y;
                            }
                            //比较设置另一个轴的大小值
                            if (rectChild.sizeDelta.x > curAxis.rect.width)
                            {
                                curAxis.rect.width = rectChild.sizeDelta.x;
                            }
                        }
                    }
                }
            }
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            _contentRect.Set(0, 0, 0, 0);

            if (constraint == Constraint.FixedWidth)
            {
                _contentRect.width = rectTransform.sizeDelta.x - padding.horizontal;
            }
            else
            {
                _contentRect.height = rectTransform.sizeDelta.y - padding.vertical;
            }

            CalculateAxes();

            if (constraint == Constraint.FixedHeight)
            {
                foreach (Axis axis in _axes)
                {
                    if (!axis.enable)
                    {
                        continue;
                    }
                    _contentRect.width += axis.rect.width + spacing.x;
                }
                _contentRect.width -= spacing.x;
            }

            SetLayoutInputForAxis(_contentRect.width + padding.horizontal, _contentRect.width + padding.horizontal,
                -1, 0);
        }

        private void CalculateContentRectPosition()
        {
            if (constraint == Constraint.FixedWidth)
            {
                _contentRect.y = padding.top;
                
                if ((int) childAlignment % 3 == 0)
                {
                    _contentRect.x = padding.left;
                }
                else if ((int) childAlignment % 3 == 1)
                {
                    _contentRect.x = (rectTransform.sizeDelta.x - _contentRect.width) / 2f + padding.left - padding.right;
                }
                else if ((int) childAlignment % 3 == 2)
                {
                    _contentRect.x = rectTransform.sizeDelta.x - _contentRect.width - padding.right;
                }
            }
            else
            {
                _contentRect.x = padding.left;
                    
                if ((int) childAlignment / 3 == 0)
                {
                    _contentRect.y = padding.top;
                }
                else if ((int) childAlignment / 3 == 1)
                {
                    _contentRect.y = (rectTransform.sizeDelta.y - _contentRect.height) / 2f + padding.top - padding.bottom;
                }
                else if ((int) childAlignment / 3 == 2)
                {
                    _contentRect.y = rectTransform.sizeDelta.y - _contentRect.height - padding.bottom;
                }
            }
        }

        public override void CalculateLayoutInputVertical()
        {
            if (constraint == Constraint.FixedWidth)
            {
                foreach (Axis axis in _axes)
                {
                    if (!axis.enable)
                    {
                        continue;
                    }
                    _contentRect.height += axis.rect.height + spacing.y;
                }
                _contentRect.height -= spacing.y;
            }

            SetLayoutInputForAxis(_contentRect.height + padding.vertical, _contentRect.height + padding.vertical, -1,
                1);

            CalculateContentRectPosition();
        }

        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

        private void SetCellsAlongAxis(int axis)
        {
            if (axis == 0)
            {
                return;
            }

            if (_axes.Count == 0)
            {
                return;
            }

            Axis preAxis = _axes[0];
            for (int i = 0; i < _axes.Count; i++)
            {
                Axis curAxis = _axes[i];
                if (!curAxis.enable)
                {
                    continue;
                }
                if (i > 1)
                {
                    preAxis = _axes[i - 1];
                }
                if (constraint == Constraint.FixedWidth)
                {
                    if ((int) childAlignment % 3 == 0)
                    {
                        curAxis.rect.x = _contentRect.x;
                    }
                    else if ((int) childAlignment % 3 == 1)
                    {
                        curAxis.rect.x = _contentRect.center.x - curAxis.rect.width / 2f;
                    }
                    else if ((int) childAlignment % 3 == 2)
                    {
                        curAxis.rect.x = _contentRect.x + _contentRect.width - curAxis.rect.width;
                    }

                    if (preAxis.Equals(curAxis))
                    {
                        curAxis.rect.y = _contentRect.y;
                    }
                    else
                    {
                        curAxis.rect.y = preAxis.rect.y + preAxis.rect.height + spacing.y;
                    }
                }
                else
                {
                    if ((int) childAlignment / 3 == 0)
                    {
                        curAxis.rect.y = _contentRect.y;
                    }
                    else if ((int) childAlignment / 3 == 1)
                    {
                        curAxis.rect.y = _contentRect.center.y - curAxis.rect.height / 2f;
                    }
                    else if ((int) childAlignment / 3 == 2)
                    {
                        curAxis.rect.y = _contentRect.y + _contentRect.height - curAxis.rect.height;
                    }

                    if (preAxis.Equals(curAxis))
                    {
                        curAxis.rect.x = _contentRect.x;
                    }
                    else
                    {
                        curAxis.rect.x = preAxis.rect.x + preAxis.rect.width + spacing.x;
                    }
                }
                curAxis.SetChildrenAlongAxis(SetChildAlongAxis, constraint, childAlignment, spacing);
                curAxis.Disable();
            }
        }
    }
}