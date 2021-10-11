using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using RestSharp;
using RestSharp.Serialization.Json;
using System.Net;

namespace Paiz
{
    class ImgurAPI
    {
        readonly string ImgurClientID = "3865d1719b5ce68";

        readonly string ImgurClientSecret = "51bc4b49e2942b03fff182e91156f75cf342887c";

        readonly string SauceNaoAPIKey = "87622f7a151e8e2ecd5a92096061624afbb62bab";

        //readonly string filepath = @"Y:\Media\JPEG\23czl25roar11.jpeg";

        public string resulturl = ""; 

        public string Upload(string path)
        {
            Testarino(path);
            return resulturl;
        }

        private void Testarino(string path)
        {
            try
            {
                RestClient rest = new RestClient("https://api.imgur.com/3/");

                var request = new RestRequest("image", Method.POST);
                request.AddHeader("Authorization", "Client-ID " + ImgurClientID);
                request.AlwaysMultipartFormData = true;
                request.AddParameter("image", Convert.ToBase64String(File.ReadAllBytes(path)));

                var response = rest.Execute(request);
                var des = new JsonDeserializer();
                ImgurDataResponse<ImgurImage> respo = des.Deserialize<ImgurDataResponse<ImgurImage>>(response);

                if (respo.Success)
                {
                    resulturl = respo.Data.Link;
                }
                else
                {
                    resulturl = "";
                }
            }
            catch (Exception Ex)
            {
                Logger.Write("Error Uploading file to imgur");
                Logger.Write(Ex.Message);
                resulturl = "";
            }
                   
        }
    }

    internal sealed class ImgurImage
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public int Datetime { get; set; }


        public string Type { get; set; }

        public bool Animated { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public int Size { get; set; }

        public long Views { get; set; }

        public long Bandwidth { get; set; }

        public string Deletehash { get; set; }

        public object Section { get; set; }

        public string Link { get; set; }
    }

    internal sealed class ImgurDataResponse<T>
    {
        public T Data { get; set; }

        public bool Success { get; set; }

        public int Status { get; set; }
    }
}
