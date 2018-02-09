namespace AgileObjects.AgileMapper.Configuration
{
    using System;
    using Extensions.Internal;

    internal class EnumMemberPair
    {
        private readonly Type _pairingEnumType;
        private readonly Type _pairedEnumType;

        private EnumMemberPair(
            Type pairingEnumType,
            string pairingEnumMemberName,
            Type pairedEnumType,
            string pairedEnumMemberName)
        {
            PairingEnumMemberName = pairingEnumMemberName;
            PairedEnumMemberName = pairedEnumMemberName;
            _pairingEnumType = pairingEnumType;
            _pairedEnumType = pairedEnumType;
        }

        public static EnumMemberPair For<TFirstEnum, TSecondEnum>(
            TFirstEnum pairingEnumMember,
            TSecondEnum pairedEnumMember)
        {
            var pairingValue = pairingEnumMember.ToConstantExpression();
            var pairedValue = pairedEnumMember.ToConstantExpression();

            return new EnumMemberPair(
                pairingValue.Type,
                pairingEnumMember.ToString(),
                pairedValue.Type,
                pairedEnumMember.ToString());
        }

        public string PairingEnumMemberName { get; }

        public string PairedEnumMemberName { get; }

        public bool IsFor(Type sourceEnumType, Type targetEnumType)
            => (_pairingEnumType == sourceEnumType) && (_pairedEnumType == targetEnumType);
    }
}