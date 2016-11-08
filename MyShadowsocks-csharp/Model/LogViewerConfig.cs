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
        public string FontName { get; set; } = "Consolas";
        public float FontSize { get; set; } = 8;
        public string BgColor { get; set; } = "black";
        public string TextColor { get; set; } = "white";
        public bool TopMost { get; set; } = false;
        public bool WrapText { get; set; } = false;
        public bool ToolbarShown { get; set; } = false;
        public int Width { get; set; } = 600;
        public int Height { get; set; } = 400;
        public int Top { get; set; } = 0;
        public int Left { get; set; } = 0;
        public bool Maximized { get; set; } = true;

        public LogViewerConfiguration()
        {
            Left = GetBestLeft();
            Top = GetBestTop();
        }

        private int GetBestLeft()
        {
            Width = (Width >= 400) ? Width : 400;
            return Screen.PrimaryScreen.WorkingArea.Width - Width;
        }

        private int GetBestTop()
        {
            Height = (Height >= 200) ? Height : 200;
            return Screen.PrimaryScreen.WorkingArea.Height - Height;
        }

        private Font GetFont()
        {
            try
            {
                return new Font(FontName, FontSize, FontStyle.Regular);
            }
            catch (Exception)
            {
                return new Font("Console", 8F);
            }
        }
        
        public void SetFont(Font font)
        {
            FontName = font.Name;
            FontSize = font.Size;
        }

        private Color GetBackgroundColor()
        {
            try
            {
                return ColorTranslator.FromHtml(BgColor);
            }
            catch (Exception)
            {
                return ColorTranslator.FromHtml("black");
            }
        }

        public void SetBackgroundColor(Color color)
        {
            BgColor = ColorTranslator.ToHtml(color);
        }

        public Color GetTextColor()
        {
            try
            {
                return ColorTranslator.FromHtml(TextColor);
            }
            catch (Exception)
            {
                return ColorTranslator.FromHtml("white");
            }
        }

        public void SetTextColor(Color color)
        {
            TextColor = ColorTranslator.ToHtml(color);
        }
    }
}
