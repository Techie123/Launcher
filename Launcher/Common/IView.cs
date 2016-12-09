using System;

namespace Launcher
{
    public interface IView
    {
        void Attach(string changelog, VersionInfo[] versions);

        void Invoke(Action action);

        void SetPlayMode(PlayMode mode);

        void SetProgress(int progress);

        void SetStatus(Status status);

        void SetStatusText(string text);

        void ShowError(string message);
    }
}
