using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [ExecuteAlways]
    public class WrapLayout : MonoBehaviour
    {
        [SerializeField] public RectOffset padding = new RectOffset();

        [SerializeField] public TextAnchor childAlignment = TextAnchor.UpperLeft;

        [SerializeField] public Vector2 spacing = Vector2.zero;

        [SerializeField] public Constraint constraint = Constraint.FixedWidth;

        [SerializeField] public List<RectTransform> ignoreList = new List<RectTransform>();

        private RectTransform _rectTransform;

        private List<RectTransform> _rectChildren = new List<RectTransform>();

#if DEVELOP_TEST
        [SerializeField]
#endif
        private Rect _contentRect = Rect.zero;
#if DEVELOP_TEST
        [SerializeField]
#endif
        private List<Axis> _axes = new List<Axis>();

        private WrapLayout()
        {
        }

        private void Awake()
        {
            _rectTransform = (RectTransform) transform;
        }

        private void Start()
        {
            RebuildLayout();
        }

        public void RebuildLayout()
        {
            _rectChildren.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }
                RectTransform rectChild = (RectTransform) child;
                if (!rectChild)
                {
                    continue;
                }
                if (ignoreList.Contains(rectChild))
                {
                    continue;
                }
                _rectChildren.Add(rectChild);
            }

            _contentRect.Set(0, 0, 0, 0);
            GetConstraintLength();
            CalculateAxesSize();
            CalculateContentRect();
            _rectTransform.sizeDelta = new Vector2(_contentRect.width + padding.horizontal,
                _contentRect.height + padding.vertical);
            CalculateAxesPosition();
        }

        private void GetConstraintLength()
        {
            if (constraint == Constraint.FixedWidth)
            {
                _contentRect.width = _rectTransform.sizeDelta.x - padding.horizontal;
            }
            else
            {
                _contentRect.height = _rectTransform.sizeDelta.y - padding.vertical;
            }
        }

        private void CalculateContentRect()
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

                _contentRect.y = padding.top;

                if ((int) childAlignment % 3 == 0)
                {
                    _contentRect.x = padding.left;
                }
                else if ((int) childAlignment % 3 == 1)
                {
                    _contentRect.x = (_rectTransform.sizeDelta.x - _contentRect.width) / 2f + padding.left -
                                     padding.right;
                }
                else if ((int) childAlignment % 3 == 2)
                {
                    _contentRect.x = _rectTransform.sizeDelta.x - _contentRect.width - padding.right;
                }
            }
            else
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

                _contentRect.x = padding.left;

                if ((int) childAlignment / 3 == 0)
                {
                    _contentRect.y = padding.top;
                }
                else if ((int) childAlignment / 3 == 1)
                {
                    _contentRect.y = (_rectTransform.sizeDelta.y - _contentRect.height) / 2f + padding.top -
                                     padding.bottom;
                }
                else if ((int) childAlignment / 3 == 2)
                {
                    _contentRect.y = _rectTransform.sizeDelta.y - _contentRect.height - padding.bottom;
                }
            }
        }

        private void CalculateAxesSize()
        {
            if (_axes.Count == 0)
            {
                _axes.Add(new Axis(0));
            }
            Axis curAxis = _axes[0];
            curAxis.Enable();

            for (int i = 0; i < _rectChildren.Count; i++)
            {
#if DEVELOP_TEST
                if (_rectChildren[i].GetComponentInChildren<Text>())
                {
                    _rectChildren[i].GetComponentInChildren<Text>().text = i.ToString();
                }
#endif
                RectTransform rectChild = _rectChildren[i];
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
                            if (_rectChildren[_rectChildren.Count - 1] == rectChild) continue;
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
                                if (_rectChildren[_rectChildren.Count - 1] == rectChild) continue;
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
                                if (_rectChildren[_rectChildren.Count - 1] == rectChild) continue;
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
                            if (_rectChildren[_rectChildren.Count - 1] == rectChild)
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
                            if (_rectChildren[_rectChildren.Count - 1] == rectChild) continue;
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
                                if (_rectChildren[_rectChildren.Count - 1] == rectChild) continue;
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
                                if (_rectChildren[_rectChildren.Count - 1] == rectChild) continue;
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
                            if (_rectChildren[_rectChildren.Count - 1] == rectChild)
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

        private void CalculateAxesPosition()
        {
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

        private void SetChildAlongAxis(RectTransform rect, int axis, float pos)
        {
            if (rect == null)
                return;

            SetChildAlongAxisWithScale(rect, axis, pos, 1.0f);
        }

        private void SetChildAlongAxisWithScale(RectTransform rect, int axis, float pos, float scaleFactor)
        {
            if (rect == null)
                return;

            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.up;

            Vector2 anchoredPosition = rect.anchoredPosition;
            anchoredPosition[axis] = (axis == 0)
                ? (pos + rect.sizeDelta[axis] * rect.pivot[axis] * scaleFactor)
                : (-pos - rect.sizeDelta[axis] * (1f - rect.pivot[axis]) * scaleFactor);
            rect.anchoredPosition = anchoredPosition;
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (!Application.isPlaying)
            {
                RebuildLayout();
            }
        }
#endif
    }
}