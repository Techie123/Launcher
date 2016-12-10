using System;

namespace Launcher
{
    public interface IView
    {
        void Attach(string html, string changelog, VersionInfo[] versions);

        void Invoke(Action action);

        void SetPlayText(string text);

        void SetProgress(int progress);

        void SetStatus(Status status);

        void SetStatusText(string text);

        void ShowError(string message);
    }
}
