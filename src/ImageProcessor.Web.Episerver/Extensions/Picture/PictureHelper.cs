﻿using System;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using ImageProcessor.Web.Episerver.Extensions.Picture;
using ImageProcessor.Web.Episerver.Picture;

namespace ImageProcessor.Web.Episerver
{
	public static class PictureHelper
    {
        public static IHtmlString Picture(this HtmlHelper helper, ContentReference imageReference, ImageType imageType, LazyLoadType lazyLoadType, string altText = "")
        {
            return Picture(helper, imageReference, imageType, string.Empty, lazyLoadType, altText);
        }

        public static IHtmlString Picture(this HtmlHelper helper, ContentReference imageReference, ImageType imageType, string cssClass = "", LazyLoadType lazyLoadType = LazyLoadType.None, string altText = "")
        {
            if (imageReference == null)
            {
                return new MvcHtmlString(string.Empty);
            }

            var pictureData = PictureUtils.GetPictureData(imageReference, imageType, lazyLoadType == LazyLoadType.CustomProgressive, altText);
            var pictureElement = BuildPictureElement(pictureData, cssClass, lazyLoadType);

            return new MvcHtmlString(HttpUtility.HtmlDecode(pictureElement));
        }

        public static IHtmlString Picture(this HtmlHelper helper, string imageUrl, ImageType imageType, LazyLoadType lazyLoadType, string altText = "")
        {
            return Picture(helper, imageUrl, imageType, string.Empty, lazyLoadType, altText);
        }

        public static IHtmlString Picture(this HtmlHelper helper, string imageUrl, ImageType imageType, string cssClass = "", LazyLoadType lazyLoadType = LazyLoadType.None, string altText = "")
        {
            if (imageUrl == null)
            {
                return new MvcHtmlString(string.Empty);
            }

            var urlBuilder = new UrlBuilder(imageUrl);

            return Picture(helper, urlBuilder, imageType, cssClass, lazyLoadType, altText);
        }

        public static IHtmlString Picture(this HtmlHelper helper, UrlBuilder imageUrl, ImageType imageType, string cssClass = "", LazyLoadType lazyLoadType = LazyLoadType.None, string altText = "")
        {
            if (imageUrl == null)
            {
                return new MvcHtmlString(string.Empty);
            }

            var pictureData = PictureUtils.GetPictureData(imageUrl, imageType, lazyLoadType == LazyLoadType.CustomProgressive, altText);
            var pictureElement = BuildPictureElement(pictureData, cssClass, lazyLoadType);

            return new MvcHtmlString(HttpUtility.HtmlDecode(pictureElement));
        }

        private static string BuildPictureElement(PictureData pictureData, string cssClass, LazyLoadType lazyLoadType)
        {
            //Create picture element
            var pictureElement = new TagBuilder("picture");

            if (pictureData.SrcSet != null)
            {
                if (pictureData.SrcSetWebp != null)
                {
                    //Add source element with webp versions. Needs to be rendered before jpg version, browser selects the first version it supports.
                    pictureElement.InnerHtml += BuildSourceElement(pictureData, lazyLoadType, "webp");
                }

                //Add source element to picture element
                pictureElement.InnerHtml += BuildSourceElement(pictureData, lazyLoadType);
            }

            //Add img element to picture element
            pictureElement.InnerHtml += BuildImgElement(pictureData, lazyLoadType, cssClass);

            return pictureElement.ToString();
        }

        private static string BuildImgElement(PictureData pictureData, LazyLoadType lazyLoadType, string cssClass)
	    {
			var imgElement = new TagBuilder("img");
		    imgElement.Attributes.Add("alt", HttpUtility.HtmlEncode(pictureData.AltText));

			//Add src and/or data-src attribute
		    switch (lazyLoadType)
		    {
			    case LazyLoadType.Custom:
                case LazyLoadType.Hybrid:
                    imgElement.Attributes.Add("data-src", pictureData.ImgSrc);
				    break;
			    case LazyLoadType.CustomProgressive:
				    imgElement.Attributes.Add("src", pictureData.ImgSrcLowQuality);
					imgElement.Attributes.Add("data-src", pictureData.ImgSrc);
				    break;
			    default:
				    imgElement.Attributes.Add("src", pictureData.ImgSrc);
				    break;
		    }

            if (lazyLoadType == LazyLoadType.Native || lazyLoadType == LazyLoadType.Hybrid)
            {
                //Add loading attribute
                imgElement.Attributes.Add("loading", "lazy");
            }

			//Add class attribute
		    if (!string.IsNullOrEmpty(cssClass))
		    {
			    imgElement.Attributes.Add("class", cssClass);
		    }

            if (pictureData.ImgDecoding != ImageDecoding.None)
            {
                imgElement.Attributes.Add("decoding", Enum.GetName(typeof(ImageDecoding), pictureData.ImgDecoding)?.ToLower());
            }

			return imgElement.ToString(TagRenderMode.SelfClosing);
		}

        private static string BuildSourceElement(PictureData pictureData, LazyLoadType lazyLoadType, string format = "")
        {
            var sourceElement = new TagBuilder("source");

            var srcset = pictureData.SrcSet;
			if (format == "webp")
            {
                srcset = pictureData.SrcSetWebp;
                sourceElement.Attributes.Add("type", "image/" + format);
            }

	        switch (lazyLoadType)
	        {
		        case LazyLoadType.Custom:
                case LazyLoadType.Hybrid:
                    sourceElement.Attributes.Add("data-srcset", srcset);
			        break;
		        case LazyLoadType.CustomProgressive:
			        sourceElement.Attributes.Add("srcset", format == "webp" ? pictureData.SrcSetLowQualityWebp : pictureData.SrcSetLowQuality);
			        sourceElement.Attributes.Add("data-srcset", srcset);
			        break;
		        default:
			        sourceElement.Attributes.Add("srcset", srcset);
			        break;
	        }

            //Add sizes attribute
            sourceElement.Attributes.Add("sizes", pictureData.SizesAttribute);

            return sourceElement.ToString(TagRenderMode.SelfClosing);
        }
    }
}
