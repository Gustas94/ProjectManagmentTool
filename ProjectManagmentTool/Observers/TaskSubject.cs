using System.Collections.Generic;
using ProjectManagmentTool.Data;

namespace ProjectManagmentTool.Observers
{
    public class TaskSubject
    {
        private readonly List<ITaskObserver> _observers = new();

        public void Attach(ITaskObserver observer)
        {
            _observers.Add(observer);
        }

        public void Detach(ITaskObserver observer)
        {
            _observers.Remove(observer);
        }

        public void Notify(ProjectTask task)
        {
            foreach (var observer in _observers)
            {
                observer.OnTaskCreated(task);
            }
        }
    }
}
