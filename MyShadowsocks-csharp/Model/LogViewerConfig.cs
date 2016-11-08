using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Shadowsocks.Model
{
    [Serializable]
    public class LogViewerConfiguration
    {
        private string fontName;
        private float fontSize;
        private string bgColor;
        private string textColor;
        private bool topMost;
        private bool wrapText;
        private bool toolbarShown;
        private int width;
        private int height;
        private int top;
        private int left;
        private bool maximized;

        public LogViewerConfiguration()
        {
            fontName = "Consolas";
            fontSize = 8;
            bgColor = "black";
            textColor = "white";
            topMost = false;
            wrapText = false;
            toolbarShown = false;
            width = 600;
            height = 400;
            left = GetBestLeft();
            top = GetBestTop();
            maximized = true;
        }

        private int GetBestLeft()
        {
            width = (width >= 400) ? width : 400;
            return Screen.PrimaryScreen.WorkingArea.Width - width;
        }

        private int GetBestTop()
        {
            height = (height >= 200) ? height : 200;
            return Screen.PrimaryScreen.WorkingArea.Height - height;
        }

        private Font GetFont()
        {
            try
            {
                return new Font(fontName, fontSize, FontStyle.Regular);
            }
            catch (Exception)
            {
                return new Font("Console", 8F);
            }
        }
        
        public void SetFont(Font font)
        {
            fontName = font.Name;
            fontSize = font.Size;
        }

        private Color GetBackgroundColor()
        {
            try
            {
                return ColorTranslator.FromHtml(bgColor);
            }
            catch (Exception)
            {
                return ColorTranslator.FromHtml("black");
            }
        }

        public void SetBackgroundColor(Color color)
        {
            bgColor = ColorTranslator.ToHtml(color);
        }

        public Color GetTextColor()
        {
            try
            {
                return ColorTranslator.FromHtml(textColor);
            }
            catch (Exception)
            {
                return ColorTranslator.FromHtml("white");
            }
        }

        public void SetTextColor(Color color)
        {
            textColor = ColorTranslator.ToHtml(color);
        }
    }
}
