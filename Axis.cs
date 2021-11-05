using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public enum Constraint
    {
        FixedWidth = 0,
        FixedHeight = 1
    }
    
#if DEVELOP_TEST
    [Serializable]
#endif
    public class Axis
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
}