using KeyViewer.Core.Interfaces;
using UnityEngine;

namespace KeyViewer.Core
{
    public abstract class ModelDrawable<T> : IDrawable where T : IModel
    {
        public T model;
        public string Name { get; protected set; }
        public ModelDrawable(T model, string name)
        {
            this.model = model;
            Name = name;
        }
        public abstract void Draw();
        public virtual void OnKeyDown(KeyCode code) { }
    }
}
