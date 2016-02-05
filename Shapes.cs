using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;

namespace TheGalleryMachine {

    public class HtmlHelperGravatar : IShapeTableProvider {
        /// <summary>
        /// Creates HTML for an <c>img</c> element that presents a Gravatar icon.
        /// </summary>
        /// <param name="Email">The email address used to identify the icon.</param>
        /// <param name="Size">An optional parameter that specifies the size of the square image in pixels.</param>
        /// <param name="Rating">An optional parameter that specifies the safety level of allowed images.</param>
        /// <param name="DefaultImage">An optional parameter that controls what image is displayed for email addresses that don't have associated Gravatar icons.</param>
        /// <returns>An HTML string of the <c>img</c> element that presents a Gravatar icon.</returns>
        [Shape]
        public static IHtmlString Gravatar(
            HtmlHelper Html,
            dynamic Shape,
            string Email,
            int? Size = null,
            GravatarRating Rating = GravatarRating.Default,
            GravatarDefaultImage DefaultImage = GravatarDefaultImage.MysteryMan) {

            var url = new StringBuilder("//www.gravatar.com/avatar/", 90);
            url.Append(GetEmailHash(Email));

            var isFirst = true;
            Action<string, string> addParam = (p, v) => {
                url.Append(isFirst ? '?' : '&');
                isFirst = false;
                url.Append(p);
                url.Append('=');
                url.Append(v);
            };

            if (Size != null) {
                if (Size < 1 || Size > 512)
                    throw new ArgumentOutOfRangeException("size", Size, "Must be null or between 1 and 512, inclusive.");
                addParam("s", Size.Value.ToString());
            }

            if (Rating != GravatarRating.Default)
                addParam("r", Rating.ToString().ToLower());

            if (DefaultImage != GravatarDefaultImage.Default) {
                if (DefaultImage == GravatarDefaultImage.Http404)
                    addParam("d", "404");
                else if (DefaultImage == GravatarDefaultImage.Identicon)
                    addParam("d", "identicon");
                if (DefaultImage == GravatarDefaultImage.MonsterId)
                    addParam("d", "monsterid");
                if (DefaultImage == GravatarDefaultImage.MysteryMan)
                    addParam("d", "mm");
                if (DefaultImage == GravatarDefaultImage.Wavatar)
                    addParam("d", "wavatar");
            }

            var tagBuilder = new TagBuilder("img");
            IDictionary<string, string> htmlAttributes = Shape.Attributes;
            IEnumerable<string> classes = Shape.Classes;
            string id = Shape.Id;

            tagBuilder.MergeAttributes(htmlAttributes, false);
            tagBuilder.Attributes.Add("src", url.ToString());

            foreach (var cssClass in classes ?? Enumerable.Empty<string>()) {
                tagBuilder.AddCssClass(cssClass);
            }

            if (!string.IsNullOrWhiteSpace(id)) {
                tagBuilder.GenerateId(id);
            }

            if (Size != null) {
                tagBuilder.Attributes.Add("width", Size.ToString());
                tagBuilder.Attributes.Add("height", Size.ToString());
            }

            return MvcHtmlString.Create(tagBuilder.ToString());
        }

        private static string GetEmailHash(string email) {
            if (email == null)
                return new string('0', 32);

            email = email.Trim().ToLower();

            var emailBytes = Encoding.ASCII.GetBytes(email);
            var hashBytes = new MD5CryptoServiceProvider().ComputeHash(emailBytes);

            Debug.Assert(hashBytes.Length == 16);

            var hash = new StringBuilder();
            foreach (var b in hashBytes)
                hash.Append(b.ToString("x2"));
            return hash.ToString();
        }

        public void Discover(ShapeTableBuilder builder) {
        }
    }

    public enum GravatarRating {
        /// <summary>
        /// The default value as specified by the Gravatar service.  That is, no rating value is specified
        /// with the request.  At the time of authoring, the default level was <see cref="G"/>.
        /// </summary>
        Default,

        /// <summary>
        /// Suitable for display on all websites with any audience type.  This is the default.
        /// </summary>
        G,

        /// <summary>
        /// May contain rude gestures, provocatively dressed individuals, the lesser swear words, or mild violence.
        /// </summary>
        Pg,

        /// <summary>
        /// May contain such things as harsh profanity, intense violence, nudity, or hard drug use.
        /// </summary>
        R,

        /// <summary>
        /// May contain hardcore sexual imagery or extremely disturbing violence.
        /// </summary>
        X
    }

    public enum GravatarDefaultImage {
        /// <summary>
        /// The default value image.  That is, the image returned when no specific default value is included
        /// with the request.  At the time of authoring, this image is the Gravatar icon.
        /// </summary>
        Default,

        /// <summary>
        /// Do not load any image if none is associated with the email hash, instead return an HTTP 404 (File Not Found) response.
        /// </summary>
        Http404,

        /// <summary>
        /// A simple, cartoon-style silhouetted outline of a person (does not vary by email hash).
        /// </summary>
        MysteryMan,

        /// <summary>
        /// A geometric pattern based on an email hash.
        /// </summary>
        Identicon,

        /// <summary>
        /// A generated 'monster' with different colors, faces, etc.
        /// </summary>
        MonsterId,

        /// <summary>
        /// Generated faces with differing features and backgrounds.
        /// </summary>
        Wavatar
    }
}