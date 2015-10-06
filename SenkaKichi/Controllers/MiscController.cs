using SenkaKichi.Models;
using Svg;
using Svg.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Linq;

namespace SenkaKichi.Controllers
{
    public class MiscController : Controller
    {
        // POST: Chart/Export
        [HttpPost, ValidateInput(false)]
        public ActionResult ExportChart(ChartModels.Export data) {
#if !DEBUG
            if (Request.Url.Host != "www.senka.me") {
                return RedirectToAction("Index", "Home");
            }
#endif
            byte[] buffer = new byte[data.Svg.Length * sizeof(char)];
            using (var image = new MemoryStream()) {
                if (data.Type == "image/svg+xml") {
                    Buffer.BlockCopy(data.Svg.ToCharArray(), 0, buffer, 0, buffer.Length);
                } else {
                    var svg = SvgDocument.FromSvg<SvgDocument>(data.Svg);
                    svg.FontFamily = "'helvetica neue', helvetica, HGPGothicM, arial, sans-serif";
                    svg.Width *= data.Scale;
                    svg.Height *= data.Scale;
                    svg.Transforms.Add(new SvgScale(data.Scale));

                    var bmp = svg.Draw();
                    if (data.Fill) {
                        var target = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format24bppRgb);
                        var g = Graphics.FromImage(target);
                        g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
                        g.DrawImage(bmp, 0, 0);
                        bmp = target;
                    }
                    bmp.Save(image, ImageFormat.Png);
                    buffer = image.ToArray();
                }
            }

            var cd = new ContentDisposition {
                FileName = data.FileName,
                Inline = false
            };
            Response.AppendHeader("Content-Disposition", cd.ToString());

            return File(buffer, data.Type);
        }
    }
}