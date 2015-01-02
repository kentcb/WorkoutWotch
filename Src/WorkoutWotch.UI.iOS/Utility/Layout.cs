// this code is a heavily modified version of https://gist.github.com/praeclarum/6225853

namespace WorkoutWotch.UI.iOS.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using MonoTouch.UIKit;

    public static class Layout
    {
        // the standard spacing between sibling views
        public static readonly int StandardSiblingViewSpacing = 8;

        // half the standard spacing between sibling views
        public static readonly int HalfSiblingViewSpacing = StandardSiblingViewSpacing / 2;

        // the standard spacing between a view and its superview
        public static readonly int StandardSuperviewSpacing = 20;

        // half the standard spacing between superviews
        public static readonly int HalfSuperviewSpacing = StandardSuperviewSpacing / 2;

        public static readonly float RequiredPriority = 1000;

        public static readonly float HighPriority = 750;

        public static readonly float LowPriority = 250;

        public static void ConstrainLayout(this UIView view, Expression<Func<bool>> constraintsExpression)
        {
            var body = constraintsExpression.Body;
            var constraints = FindBinaryExpressionsRecursive(body)
                .Select(e => CompileConstraint(e, view))
                .ToArray();
                
            view.AddConstraints(constraints);
        }

        private static IEnumerable<BinaryExpression> FindBinaryExpressionsRecursive(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;

            if (binaryExpression == null)
            {
                yield break;
            }

            if (binaryExpression.NodeType == ExpressionType.AndAlso)
            {
                foreach (var childBinaryExpression in FindBinaryExpressionsRecursive(binaryExpression.Left))
                {
                    yield return childBinaryExpression;
                }

                foreach (var childBinaryExpression in FindBinaryExpressionsRecursive(binaryExpression.Right))
                {
                    yield return childBinaryExpression;
                }
            }
            else
            {
                yield return binaryExpression;
            }
        }

        private static NSLayoutConstraint CompileConstraint(BinaryExpression binaryExpression, UIView constrainedView)
        {
            NSLayoutRelation layoutRelation;

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.Equal:
                    layoutRelation = NSLayoutRelation.Equal;
                    break;
                case ExpressionType.LessThanOrEqual:
                    layoutRelation = NSLayoutRelation.LessThanOrEqual;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    layoutRelation = NSLayoutRelation.GreaterThanOrEqual;
                    break;
                default:
                    throw new NotSupportedException("Not a valid relationship for a constraint: " + binaryExpression.NodeType);
            }

            UIView leftView;
            NSLayoutAttribute leftLayoutAttribute;
            DetermineConstraintInformationFromExpression(binaryExpression.Left, out leftView, out leftLayoutAttribute);

            if (leftView != constrainedView)
            {
                leftView.TranslatesAutoresizingMaskIntoConstraints = false;
            }

            UIView rightView;
            NSLayoutAttribute rightLayoutAttribute;
            float multiplier;
            float constant;
            DetermineConstraintInformationFromExpression(binaryExpression.Right, out rightView, out rightLayoutAttribute, out multiplier, out constant);

            if (rightView != null && rightView != constrainedView)
            {
                rightView.TranslatesAutoresizingMaskIntoConstraints = false;
            }

            return NSLayoutConstraint.Create(
                leftView,
                leftLayoutAttribute,
                layoutRelation,
                rightView,
                rightLayoutAttribute,
                multiplier,
                constant);
        }

        private static void DetermineConstraintInformationFromExpression(Expression expression, out UIView view, out NSLayoutAttribute layoutAttribute)
        {
            var methodCallExpression = FindExpressionOfType<MethodCallExpression>(expression);

            if (methodCallExpression == null)
            {
                throw new NotSupportedException("Constraint expression must be a method call.");
            }

            MemberExpression viewExpression;
            layoutAttribute = NSLayoutAttribute.NoAttribute;

            switch (methodCallExpression.Method.Name)
            {
                case "Width":
                    layoutAttribute = NSLayoutAttribute.Width;
                    break;
                case "Height":
                    layoutAttribute = NSLayoutAttribute.Height;
                    break;
                case "Left":
                case "X":
                    layoutAttribute = NSLayoutAttribute.Left;
                    break;
                case "Top":
                case "Y":
                    layoutAttribute = NSLayoutAttribute.Top;
                    break;
                case "Right":
                    layoutAttribute = NSLayoutAttribute.Right;
                    break;
                case "Bottom":
                    layoutAttribute = NSLayoutAttribute.Bottom;
                    break;
                case "CenterX":
                    layoutAttribute = NSLayoutAttribute.CenterX;
                    break;
                case "CenterY":
                    layoutAttribute = NSLayoutAttribute.CenterY;
                    break;
                case "Baseline":
                    layoutAttribute = NSLayoutAttribute.Baseline;
                    break;
                case "Leading":
                    layoutAttribute = NSLayoutAttribute.Leading;
                    break;
                case "Trailing":
                    layoutAttribute = NSLayoutAttribute.Trailing;
                    break;
                default:
                    throw new NotSupportedException("Method call '" + methodCallExpression.Method.Name + "' is not recognized as a valid constraint.");
            }

            if (methodCallExpression.Arguments.Count != 1)
            {
                throw new NotSupportedException("Method call '" + methodCallExpression.Method.Name + "' has " + methodCallExpression.Arguments.Count + " arguments, where only 1 is allowed.");
            }

            viewExpression = methodCallExpression.Arguments.FirstOrDefault() as MemberExpression;

            if (viewExpression == null)
            {
                throw new NotSupportedException("The argument to method call '" + methodCallExpression.Method.Name + "' must be a member expression that resolves to the view being constrained.");
            }
                
            view = Evaluate<UIView>(viewExpression);

            if (view == null)
            {
                throw new NotSupportedException("The argument to method call '" + methodCallExpression.Method.Name + "' resolved to null, so the view to be constrained could not be determined.");
            }
        }

        private static void DetermineConstraintInformationFromExpression(Expression expression, out UIView view, out NSLayoutAttribute layoutAttribute, out float multiplier, out float constant)
        {
            var viewExpression = expression;

            view = null;
            layoutAttribute = NSLayoutAttribute.NoAttribute;
            multiplier = 1.0f;
            constant = 0.0f;

            if (viewExpression.NodeType == ExpressionType.Add || viewExpression.NodeType == ExpressionType.Subtract)
            {
                var binaryExpression = (BinaryExpression)viewExpression;
                constant = Evaluate<float>(binaryExpression.Right);

                if (viewExpression.NodeType == ExpressionType.Subtract)
                {
                    constant = -constant;
                }

                viewExpression = binaryExpression.Left;
            }

            if (viewExpression.NodeType == ExpressionType.Multiply || viewExpression.NodeType == ExpressionType.Divide)
            {
                var binaryExpression = (BinaryExpression)viewExpression;
                multiplier = Evaluate<float>(binaryExpression.Right);

                if (viewExpression.NodeType == ExpressionType.Divide)
                {
                    multiplier = 1 / multiplier;
                }

                viewExpression = binaryExpression.Left;
            }

            if (viewExpression is MethodCallExpression)
            {
                DetermineConstraintInformationFromExpression(viewExpression, out view, out layoutAttribute);
            }
            else
            {
                // constraint must be something like: view.Width() == 50
                constant = Evaluate<float>(viewExpression);
            }
        }

        private static T Evaluate<T>(Expression expression)
        {
            var result = Evaluate(expression);

            if (result is T)
            {
                return (T)result;
            }

            return (T)Convert.ChangeType(Evaluate(expression), typeof(T));
        }

        private static object Evaluate(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expression).Value;
            }

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)expression;
                var member = memberExpression.Member;

                if (member.MemberType == MemberTypes.Field)
                {
                    var fieldInfo = (FieldInfo)member;

                    if (fieldInfo.IsStatic)
                    {
                        return fieldInfo.GetValue(null);
                    }
                }
            }

            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        // searches for an expression of type T within expression, skipping through "irrelevant" nodes
        private static T FindExpressionOfType<T>(Expression expression)
            where T : Expression
        {
            while (!(expression is T))
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Convert:
                        expression = ((UnaryExpression)expression).Operand;
                        break;
                    default:
                        return default(T);
                }
            }

            return (T)expression;
        }
    }
}