namespace AgileObjects.AgileMapper.UnitTests.Configuration.Inline
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if !NETCOREAPP1_0
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;
#endif
    using Common;
    using Common.TestClasses;
    using TestClasses;
#if !NET35
    using Xunit;
#else
    using Fact = NUnit.Framework.TestAttribute;

    [NUnit.Framework.TestFixture]
#endif
    public class WhenConfiguringConstructorDataSourcesInline
    {
        [Fact]
        public void ShouldApplyAConfiguredConstantByParameterTypeInline()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var result = mapper
                    .Map(new PublicProperty<Guid> { Value = Guid.NewGuid() })
                    .ToANew<PublicCtor<string>>(cfg => cfg
                        .Map("Hello there!")
                        .ToCtor<string>());

                result.Value.ShouldBe("Hello there!");
            }
        }

        [Fact]
        public void ShouldExtendConstructorDataSourceConfiguration()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicTwoFieldsStruct<int, int>>()
                    .To<PublicTwoParamCtor<int, int>>()
                    .Map(ctx => ctx.Source.Value1 * 2)
                    .ToCtor("value1");

                var result1 = mapper
                    .Map(new PublicTwoFieldsStruct<int, int> { Value1 = 2, Value2 = 6 })
                    .ToANew<PublicTwoParamCtor<int, int>>(cfg => cfg
                        .Map(ctx => ctx.Source.Value2 / 2)
                        .ToCtor("value2"));

                result1.Value1.ShouldBe(4);
                result1.Value2.ShouldBe(3);

                var result2 = mapper
                    .Map(new PublicTwoFieldsStruct<int, int> { Value1 = 3, Value2 = 8 })
                    .ToANew<PublicTwoParamCtor<int, int>>(cfg => cfg
                        .Map(ctx => ctx.Source.Value2 / 2)
                        .ToCtor("value2"));

                result2.Value1.ShouldBe(6);
                result2.Value2.ShouldBe(4);
            }
        }

        [Fact]
        public void ShouldReplaceAConfiguredConstructorDataSource()
        {
            using (var mapper = Mapper.CreateNew())
            {
                mapper.WhenMapping
                    .From<PublicProperty<long>>()
                    .To<PublicCtor<string>>()
                    .Map((s, t) => s.Value * 2)
                    .ToCtor<string>();

                var moreThanTwoResult = mapper
                    .Map(new PublicProperty<long> { Value = 3 })
                    .ToANew<PublicCtor<string>>(cfg => cfg
                        .Map((s, t) => s.Value > 2 ? 2 : 1)
                        .ToCtor<string>());

                moreThanTwoResult.Value.ShouldBe("2");

                var lessThanTwoResult = mapper
                    .Map(new PublicProperty<long> { Value = 0 })
                    .ToANew<PublicCtor<string>>(cfg => cfg
                        .Map((s, t) => s.Value > 2 ? 2 : 1)
                        .ToCtor<string>());

                lessThanTwoResult.Value.ShouldBe("1");
            }
        }

#if !NETCOREAPP1_0
        // See https://github.com/agileobjects/AgileMapper/issues/209
        [Fact]
        public void ShouldDeepCloneViaConstructor()
        {
            using (var mapper = Mapper.CreateNew())
            {
                var preferences = new[]
                {
                    Issue209.Preference.ActivitiesAttendanceTabLimitDays,
                    Issue209.Preference.MembersNameFormat,
                    Issue209.Preference.ReportTagLine,
                    Issue209.Preference.StartupSyncAddrAtLogin
                };

                var source = new Issue209.ViewDataGridPreferencesDto(
                    new List<Issue209.Preference>(preferences),
                    new List<Issue209.UserPreferenceDto>
                    {
                        new Issue209.UserPreferenceDto
                        {
                            OptionList = new BindingList<Issue209.PreferenceOptionDto>
                            {
                                new Issue209.PreferenceOptionDto(),
                                new Issue209.PreferenceOptionDto(),
                            },
                            SelectedOption = new Issue209.PreferenceOptionDto()
                        },
                        new Issue209.UserPreferenceDto
                        {
                            OptionList = new BindingList<Issue209.PreferenceOptionDto>
                            {
                                new Issue209.PreferenceOptionDto()
                            },
                            SelectedOption = new Issue209.PreferenceOptionDto()
                        }
                    },
                    Issue209.GridPreferenceCallerSource.ActivityRecord);

                var result = mapper.DeepClone(source, cfg => cfg
                    .Map((vds, vdt) => vds.Source)
                    .ToCtor<Issue209.GridPreferenceCallerSource>());

                result.ShouldNotBeNull();

                result.PreferenceList.SequenceEqual(preferences).ShouldBeTrue();

                result.PreferenceDtoList.Count.ShouldBe(2);
                result.PreferenceDtoList.First().OptionList.Count.ShouldBe(2);
                result.PreferenceDtoList.First().SelectedOption.ShouldNotBeNull();
                result.PreferenceDtoList.Second().OptionList.ShouldHaveSingleItem();
                result.PreferenceDtoList.Second().SelectedOption.ShouldNotBeNull();

                result.Source.ShouldBe(Issue209.GridPreferenceCallerSource.ActivityRecord);
            }
        }
#endif
        #region Helper Members


#if !NETCOREAPP1_0
        #region Issue 209

        public static class Issue209
        {
            public class ObservableObject
            {
                private PropertyInfo[] _properties;
                public PropertyInfo[] Properties => _properties ??= GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            public class DtoBase : ObservableObject
            {
                #region Properties

                private bool _isDirty;
                public bool IsDirty
                {
                    get => _isDirty;
                    set
                    {
                        if (_isDirty == value)
                            return;

                        _isDirty = value;

                        RaisePropertyChanged(nameof(IsDirty));
                    }
                }

                public Dictionary<string, object> OriginalPropertyValues { get; set; }

                public bool IsLinkedToDatabase { get; set; }

                #endregion

                #region Public Methods

                // ReSharper disable once RedundantAssignment
                protected bool SetValue<TT>(ref TT oldValue, TT newValue, string propertyName = null)
                {
                    if (EqualityComparer<TT>.Default.Equals(oldValue, newValue))
                        return false;

                    oldValue = newValue;

                    RaisePropertyChanged(propertyName);
                    return true;
                }

                // ReSharper disable once RedundantAssignment
                protected bool SetValue<TT>(Expression<Func<TT>> propertyExpression, ref TT oldValue, TT newValue)
                {
                    if (EqualityComparer<TT>.Default.Equals(oldValue, newValue))
                        return false;

                    oldValue = newValue;
                    RaisePropertyChanged(propertyExpression);

                    return true;
                }

                #endregion

                #region Protected Methods

                protected void RaisePropertyChanged(string propertyName = null)
                {
                }

                protected void RaisePropertyChanged<TT>(Expression<Func<TT>> propertyExpression)
                {
                }

                #endregion
            }

            public enum GridPreferenceCallerSource
            {
                MemberRecord,
                ActivityRecord
            }

            public enum Preference
            {
                Undefined = 0,

                // Front Desk
                FrontDeskShowCompany = 1,
                FrontDeskAllowNewCheckin = 2,
                FrontDeskAllowNewDues = 3,
                FrontDeskLimitSynopsis = 4,
                FrontDeskEndDateVal = 5,
                FrontDeskPopupExpired = 6,
                FrontDeskRepeatCheckinPrompt = 7,
                FrontDeskUseUnattendedPwd = 8,
                FrontDeskResetOnCheckIn = 9,
                FrontDeskClearCheckinDetails = 10,
                // Activities
                ActivitiesCurrentBookNbr = 101,
                // 103 is the heading for attendance tab prefs
                ActivitiesAttendanceTabLimitDays = 104,

                // Address
                AddressAddressName = 201,
                // Licensed Org
                OrganizationClubName = 301,
                OrganizationFiscalYear = 302,
                OrganizationLicense = 304,
                // Database
                DatabaseBackupFrequency = 401,
                DatabaseLastBackupDate = 402,
                // No 403
                DatabaseMaxStoredFileKb = 404,
                DatabaseMaxStoredPhotoKb = 405,
                DatabaseTableMaintFreq = 406,
                DatabaseTimeoutSeconds = 407,
                DatabaseProfiles = 408,
                /// <summary>
                /// F = File Storage, D = Database Storage
                /// </summary>
                DatabaseAllFileStorageOptions = 409,
                DatabaseAutoLogin = 410,
                DatabaseWildcardCharacter = 411,
                DatabaseStaleConnectionHours = 413,
                // Donations
                DonationsCurrentBookNbr = 501,
                // Dues
                DuesAppFee = 604,
                DuesIncrementMonths = 605,
                DuesCurrentBookNbr = 606,
                DuesDefaultSyncStatus = 607,
                DuesNegativeInRed = 608,
                // Groups
                //public const int NameForGroups = 702,
                //GroupSingular = 703,
                //GroupPlural = 704,
                GroupsCurrentBookNbr = 706,
                // Login
                LoginUserName = 801,
                LoginPassword = 802,
                LoginDatabase = 803,
                // Members
                MembersShowSystemId = 901,
                MembersStartDate = 902,
                MembersEndDate = 903,
                MembersStartInFname = 904,
                MembersRecyclePreloadedId = 905,
                MembersDeleteAction = 906,
                MembersIdPrefix = 907,
                MembersSetFlagOnNew = 908,
                MembersResetEndDateOnExpire = 909,
                MembersEnableAddFromDlScan = 910,
                // MembersAttendanceTabSettings == 912 // Hidden heading for attendance tab prefs
                MembersAttendanceTabLimitDays = 913,
                MembersAttendanceTabLimitToActId = 914,
                MembersNameFormat = 915,
                MembersPreloadedIdRequired = 916,
                //MembersMemberIdHeading = 917, // Heading for Member IDs

                // Misc
                MiscPromptOnExit = 1001,
                MiscDeletePrompt = 1002,
                MiscDebitCredit = 1003,
                MiscAutoClearDues = 1004,
                MiscNumericMemId = 1006,
                MiscDefaultItemDate = 1007,
                MiscAutoCloseSearch = 1016,
                // MiscDirectoryLocHeader = 1018 // Heading for file locations
                MiscLastBackupDir = 1019,
                MiscMaxViewColumnCount = 1020,
                MiscEnableEventLog = 1021,
                MiscLastExportDir = 1022,
                MiscLastImageDir = 1023,
                MiscMaxCustomFieldCount = 1024,
                MiscEmailAttachmentDir = 1025,
                MiscEmailFormat = 1026,
                MiscEmailPreview = 1027,
                MiscLastFileRelDir = 1028,
                MiscDisplayAssignNow = 1029, // Used for any type of postings
                                             // MiscReceiptBookHeader = 1030, // Heading for receipt book numbers
                MiscLanguage = 1031,

                // Reports
                ReportPaperSize = 1101,
                ReportAddressFormat = 1102,
                ReportAltNameUsage = 1103,
                ReportAppendCurRes = 1104,
                ReportReturnAddr = 1105,
                ReportExtraInfoType = 1106,
                ReportHideCountry = 1107,
                ReportPrintedBy = 1108,
                ReportShowReceiptTagLine = 1109,
                ReportDefaultLogo = 1110,
                ReportShowCompany = 1111,
                ReportAutoIncludeCriteria = 1112,
                ReportTagLine = 1113,
                ReportActiveReport = 1114,
                ReportOrganizationFont = 1118,
                ReportReportFont = 1120,
                ReportLabelFont = 1122,
                // Shutdown
                ShutDownDeleteDeletes = 1201,
                ShutDownIdleTimeOut = 1202,
                // Startup
                StartupSyncAddrAtLogin = 1301,
                StartupToDoPrompt = 1302,
                // More startup
                StartupShowTips = 1401,
                StartupStartupList = 1404,
                StartupMessageOfTheDay = 1405,
                StartupLicensePrompt = 1406,
                StartupLastMassSetAddrDate = 1407,
                // Private Fields
                PrivateFieldLabel01 = 1502,
                PrivateFieldLabel02 = 1503,
                PrivateFieldLabel03 = 1504,
                PrivateFieldLabel04 = 1505,
                PrivateFieldLabel05 = 1506,
                PrivateFieldLabel06 = 1507,
                PrivateFieldLabel07 = 1508,
                PrivateFieldLabel08 = 1509
            }

            public class PreferenceOptionDto : DtoBase
            {
                private int _optionId;
                public int OptionId
                {
                    get => _optionId;
                    set => SetValue(ref _optionId, value);
                }

                private int _preferenceId;
                public int PreferenceId
                {
                    get => _preferenceId;
                    set => SetValue(ref _preferenceId, value);
                }

                private string _displayName;
                public string DisplayName
                {
                    get => _displayName;
                    set => SetValue(ref _displayName, value);
                }

                private string _dataValue;
                public string DataValue
                {
                    get => _dataValue;
                    set => SetValue(ref _dataValue, value);
                }

                private int _sortOrder;
                public int SortOrder
                {
                    get => _sortOrder;
                    set => SetValue(ref _sortOrder, value);
                }
            }

            public class UserPreferenceDto : DtoBase
            {
                private int _userPreferenceId;
                public int UserPreferenceId
                {
                    get => _userPreferenceId;
                    set => SetValue(ref _userPreferenceId, value);
                }

                private int _preferenceId;
                public int PreferenceId
                {
                    get => _preferenceId;
                    set => SetValue(ref _preferenceId, value);
                }

                private int _userSecId;
                public int UserSecId
                {
                    get => _userSecId;
                    set => SetValue(ref _userSecId, value);
                }

                private int _scopeId;
                public int ScopeId
                {
                    get => _scopeId;
                    set => SetValue(ref _scopeId, value);
                }

                private string _defaultValue;
                public string DefaultValue
                {
                    get => _defaultValue;
                    set => SetValue(ref _defaultValue, value);
                }

                private string _preferenceValue;
                public string PreferenceValue
                {
                    get => _preferenceValue ?? "";
                    set => SetValue(ref _preferenceValue, value);
                }

                private int _applicationId;
                public int ApplicationId
                {
                    get => _applicationId;
                    set => SetValue(ref _applicationId, value);
                }

                private string _displayLabel;
                public string DisplayLabel
                {
                    get => _displayLabel;
                    set => SetValue(ref _displayLabel, value);
                }

                public bool IsSystemPreference => ScopeId == 1;

                public bool IsUserPreference => ScopeId == 2;

                public bool IsStdOnly => ApplicationId == 1;

                public bool IsProOnly => ApplicationId == 2;

                public bool IsStdOrPro => ApplicationId == 3;

                public bool IsPreferenceValueChanged => !string.Equals(DefaultValue, PreferenceValue);

                private bool _boolValueDisplay;
                public bool BoolValueDisplay
                {
                    get => PreferenceValue.Equals("Y");
                    set
                    {
                        SetValue(ref _boolValueDisplay, value);
                        PreferenceValue = value ? "Y" : "N";
                    }
                }

                private BindingList<PreferenceOptionDto> _optionList;
                public BindingList<PreferenceOptionDto> OptionList
                {
                    get => _optionList;
                    set => SetValue(ref _optionList, value);
                }

                private PreferenceOptionDto _selectedOption;
                public PreferenceOptionDto SelectedOption
                {
                    get => _selectedOption;
                    set
                    {
                        SetValue(ref _selectedOption, value);
                        PreferenceValue = value?.DataValue;
                    }
                }
            }

            public abstract class ViewDataDto
            {
            }

            public class ViewDataGridPreferencesDto : ViewDataDto
            {
                public ViewDataGridPreferencesDto(
                    List<Preference> preferenceList,
                    List<UserPreferenceDto> preferenceDtoList,
                    GridPreferenceCallerSource callerSource)
                {
                    PreferenceList = preferenceList;
                    PreferenceDtoList = preferenceDtoList;
                    Source = callerSource;
                }

                public List<Preference> PreferenceList { get; }

                public List<UserPreferenceDto> PreferenceDtoList { get; }

                public GridPreferenceCallerSource Source { get; }

                public bool IsSettingsActive => PreferenceDtoList.Exists(p => p.IsPreferenceValueChanged);

                public Preference ActivityIdPreferenceKey => Source == GridPreferenceCallerSource.MemberRecord
                    ? Preference.MembersAttendanceTabLimitToActId
                    : Preference.Undefined;

                public Preference ActivityTimeFramePreferenceKey => Source == GridPreferenceCallerSource.MemberRecord
                    ? Preference.MembersAttendanceTabLimitDays
                    : Preference.ActivitiesAttendanceTabLimitDays;
            }
        }

        #endregion
#endif

        #endregion
    }
}
