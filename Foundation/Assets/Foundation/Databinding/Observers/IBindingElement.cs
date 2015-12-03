// -------------------------------------
//  Domain		: Avariceonline.com
//  Author		: Nicholas Ventimiglia
//  Product		: Unity3d Foundation
//  Published		: 2015
//  -------------------------------------


using Foundation.Databinding;

namespace Foundation.Databinding
{
    /// <summary>
    ///     Interface for a UI interaction that receives binding messages from the context
    /// </summary>
    public interface IBindingElement
    {
        /// <summary>
        ///     Manager
        /// </summary>
        BindingContext Context { get; }

        /// <summary>
        ///     Access to Model
        /// </summary>
        IObservableModel Model { get; set; }

        /// <summary>
        ///     Handle Updates
        /// </summary>
        /// <param name="message"></param>
        void OnBindingMessage(ObservableMessage message);

        /// <summary>
        ///     Context Refresh
        /// </summary>
        void OnBindingRefresh();
    }
}