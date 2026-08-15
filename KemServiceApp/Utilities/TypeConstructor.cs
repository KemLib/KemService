using System.Reflection;

namespace KemServiceApp.Utilities
{
    public class TypeConstructor<T> where T : class
    {
        #region Properties
        private const string ERROR_TYPE_CAN_NOT_NULL = "Type can't null",
            ERROR_TYPE_CREATE_CONSTRUCTOR_FAIL = "Type {0} created constructor fail",
            ERROR_TYPE_CREATE_CONSTRUCTOR_EXCEPTION = "Type {0} created constructor exception \"{1}\"",
            ERROR_CONSTRUCTOR_INFO_CAN_NOT_NULL = "ConstructorInfo can't null",
            ERROR_CONSTRUCTOR_INFO_CREATE_INSTANCE_FAIL = "ConstructorInfo {0} create instance fail",
            ERROR_CONSTRUCTOR_INFO_CREATE_INSTANCE_EXCEPTION = "ConstructorInfo {0} create instance exception \"{0}\"";

        private readonly ConstructorInfo? constructorInfo;
        private readonly object[]? parameters;
        #endregion

        #region Constrcution
        public TypeConstructor(ConstructorInfo? constructorInfo)
        {
            this.constructorInfo = constructorInfo;
            parameters = [];
        }
        public TypeConstructor(ConstructorInfo? constructor, object[]? parameters)
        {
            constructorInfo = constructor;
            this.parameters = parameters;
        }
        #endregion

        #region Method
        public bool TryCreateInstance([NotNullWhen(true)] out T? value, [NotNullWhen(false)] out string? error)
        {
            if (constructorInfo == null)
            {
                value = null;
                error = ERROR_CONSTRUCTOR_INFO_CAN_NOT_NULL;
                return false;
            }
            //
            try
            {
                object? obj = constructorInfo.Invoke(parameters);
                if (obj == null)
                {
                    value = null;
                    error = string.Format(ERROR_CONSTRUCTOR_INFO_CREATE_INSTANCE_FAIL, constructorInfo.Name);
                    return false;
                }
                value = (T)obj;
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                value = null;
                error = string.Format(ERROR_CONSTRUCTOR_INFO_CREATE_INSTANCE_EXCEPTION, constructorInfo.Name, ex.Message);
                return false;
            }
        }
        #endregion

        #region Method Static
        public static bool TryCreateConstructor(Type? type, [NotNullWhen(true)] out TypeConstructor<T>? constructor, [NotNullWhen(false)] out string? error)
        {
            if (type == null)
            {
                constructor = null;
                error = ERROR_TYPE_CAN_NOT_NULL;
                return false;
            }
            //
            try
            {
                Type[] parametersType = [];
                ConstructorInfo? constructorInfo = type.GetConstructor(parametersType);
                if (constructorInfo == null)
                {
                    constructor = null;
                    error = string.Format(ERROR_TYPE_CREATE_CONSTRUCTOR_FAIL, type.Name);
                    return false;
                }
                constructor = new TypeConstructor<T>(constructorInfo);
                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                constructor = null;
                error = string.Format(ERROR_TYPE_CREATE_CONSTRUCTOR_EXCEPTION, type.Name, ex.Message);
                return false;
            }
        }
        public static bool TryCreateConstructor(Type? type, object[]? parameters, [NotNullWhen(true)] out TypeConstructor<T>? constructor, [NotNullWhen(false)] out string? error)
        {
            if (type == null)
            {
                constructor = null;
                error = ERROR_TYPE_CAN_NOT_NULL;
                return false;
            }
            //
            if (parameters == null || parameters.Length == 0)
            {
                return TryCreateConstructor(type, out constructor, out error);
            }
            //
            try
            {
                Type[] parametersType = new Type[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                    parametersType[i] = parameters[i].GetType();
                //
                ConstructorInfo? constructorInfo = type.GetConstructor(parametersType);
                if (constructorInfo == null)
                {
                    constructor = null;
                    error = string.Format(ERROR_TYPE_CREATE_CONSTRUCTOR_FAIL, type.Name);
                    return false;
                }
                constructor = new TypeConstructor<T>(constructorInfo, parameters);
                error = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                constructor = null;
                error = string.Format(ERROR_TYPE_CREATE_CONSTRUCTOR_EXCEPTION, type.Name, ex.Message);
                return false;
            }
        }
        #endregion
    }
}
