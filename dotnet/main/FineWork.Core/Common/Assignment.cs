using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Common
{
    public interface IAssignable<in T>
    {
        void AssignFrom(T source);
    }

    public class AssignmentSource<TSource>
    {
        public AssignmentSource(TSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            this.Source = source;
        }

        public TSource Source { get; }

        public TTarget To<TTarget>() where TTarget : IAssignable<TSource>, new()
        {
            var info = new TTarget();
            info.AssignFrom(this.Source);
            return info;
        }
    }

    public static class AssignmentExtensions
    {
        public static AssignmentSource<TSource> Assigner<TSource>(this TSource source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return new AssignmentSource<TSource>(source);
        }
    }
}
