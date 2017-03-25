using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Svg.Editor
{
    public class EmbeddedResourceRegistry : IEmbeddedResourceRegistry
    {
        private readonly List<Type> _targetTypes;

        public EmbeddedResourceRegistry()
        {
            var types = new List<Type>();
            types.Add(typeof(EmbeddedResourceRegistry));

            // handle inherited class
            var t = GetType();
            if (!types.Contains(t))
                types.Add(t);

            _targetTypes = types.OfType<Type>().Reverse().ToList();
        }


        /// <summary>
        /// Allows to register one or more types which live in an assembly that contains embedded resources.
        /// The namespace of the type is used to match the resource, so make sure it the resources start with the same namespace as the type!
        /// </summary>
        /// <param name="type"></param>
        public void Register(params Type[] types)
        {
            foreach (var type in types.Reverse())
            {
                if (_targetTypes.Contains(type))
                    _targetTypes.Add(type);
            }
        }

        public IEnumerable<Type> EmbeddedResourceTypes => _targetTypes.ToArray();
    }

    public interface IEmbeddedResourceRegistry
    {
        void Register(params Type[] types);
        IEnumerable<Type> EmbeddedResourceTypes { get; }
    }
}
