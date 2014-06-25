﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace AonWeb.FluentHttp.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an unhandled error in the http call.
    /// </summary>
    public class TypeMismatchException : Exception
    {
        public TypeMismatchException(Type expectedType, Type actualType)
            : this(expectedType, actualType, string.Format(SR.TypeMismatchErrorFormat, expectedType.FormattedTypeName(), actualType.FormattedTypeName()))
        {
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        public TypeMismatchException(Type expectedType, Type actualType, string message)
            : base(message)
        {
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        [ExcludeFromCodeCoverage]
        public TypeMismatchException(Type expectedType, Type actualType, string message, Exception exception) :
            base(message, exception)
        {
            ExpectedType = expectedType;
            ActualType = actualType;
        }

        [ExcludeFromCodeCoverage]
        protected TypeMismatchException(SerializationInfo info, StreamingContext context) :
            base(info, context) { }

        public Type ExpectedType { get; private set; }
        public Type ActualType { get; private set; }
    }
}