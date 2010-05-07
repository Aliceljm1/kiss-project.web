using System;
using Kiss.Query;

namespace Kiss.Web.Mvc
{
    /// <summary>
    /// 通过该接口获取当前操作的一些信息
    /// </summary>
    public interface IControllerContext
    {
        /// <summary>
        /// current type
        /// </summary>
        Type CurrentModelType { get; }

        /// <summary>
        /// get obj type
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Type GetModelType(string key);

        /// <summary>
        /// current controller
        /// </summary>
        object CurrentController { get; }

        /// <summary>
        /// current controller type
        /// </summary>
        Type CurrentControllerType { get; }

        /// <summary>
        /// get mvc controller type
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Type GetControllerType(string key);

        /// <summary>
        /// create controller
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object CreateController(string key);

        /// <summary>
        /// create controller
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        object CreateController(Type modelType);

        /// <summary>
        /// current query condition
        /// </summary>
        QueryCondition CurrentQc { get; }

        /// <summary>
        /// create query condition
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        QueryCondition CreateQC(string key);

        /// <summary>
        /// current service
        /// </summary>
        IRepository CurrentService { get; }

        /// <summary>
        ///  create service of model type
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        IRepository CreateService(Type modelType);

        /// <summary>
        /// create service of key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IRepository CreateService(string key);

        /// <summary>
        /// controller name
        /// </summary>
        string ControllerName { get; }

        /// <summary>
        /// action name
        /// </summary>
        string ActionName { get; }
    }
}
