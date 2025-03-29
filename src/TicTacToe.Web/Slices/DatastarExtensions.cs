using System.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TicTacToe.Web.Slices
{
    public static class DatastarExtensions
    {
        /// <summary>
        /// Renders validation data attributes for datastar forms
        /// </summary>
        public static IHtmlContent DatastarValidationAttributes(
            this IHtmlHelper html,
            string? error = null
        )
        {
            if (string.IsNullOrEmpty(error))
                return HtmlString.Empty;

            var encodedError = HttpUtility.HtmlAttributeEncode(error);
            return new HtmlString($"data-error='{encodedError}' aria-invalid='true'");
        }

        /// <summary>
        /// Renders an error message with datastar attributes
        /// </summary>
        public static IHtmlContent DatastarValidationMessage(
            this IHtmlHelper html,
            string? error = null
        )
        {
            if (string.IsNullOrEmpty(error))
                return HtmlString.Empty;

            return new HtmlString(
                $@"
                <div class='validation-message' role='alert' data-error-message>
                    {HttpUtility.HtmlEncode(error)}
                </div>
            "
            );
        }
    }
}
