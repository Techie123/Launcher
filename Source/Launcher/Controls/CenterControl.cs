using System;
using Eto.Forms;

namespace Launcher
{
    public class CenterControl : DynamicLayout
    {
        public CenterControl(bool vertical = true)
        {
            if (vertical)
                BeginVertical();
            else
                BeginHorizontal();

            Add(null, true, true);
        }

        public void Finish()
        {
            Add(null, true, true);
        }
    }
}
