namespace CABR_ID_SCANNER
{
    public class PersonalIdCropFilePaths
    {
        private string personalIdFilepath;
        private string cnpCropFilepath;
        private string firstNameCropFilepath;
        private string middleNameCropFilepath;
        private string lastNameCropFilepath;
        private string genderCropFilepath;
        private string birthplaceCropFilepath;
        private string issuedByCropFilepath;
        private string validityCropFilepath;
        private string expireDateCropFilepath;
        private string emittedDateCropFilepath;
        private string nationalityCropFilepath;
        private string idNumberCropFilepath;
        private AddressCropFilePaths addressCropFilePaths;

        public string ExpireDateCropFilepath
        {
            get => expireDateCropFilepath;
            set => expireDateCropFilepath = value;
        }

        public string EmittedDateCropFilepath
        {
            get => emittedDateCropFilepath;
            set => emittedDateCropFilepath = value;
        }

        public string IdNumberCropFilepath
        {
            get => idNumberCropFilepath;
            set => idNumberCropFilepath = value;
        }

        public string NationalityCropFilepath
        {
            get => nationalityCropFilepath;
            set => nationalityCropFilepath = value;
        }

        public string ValidityCropFilepath
        {
            get => validityCropFilepath;
            set => validityCropFilepath = value;
        }

        public string IssuedByCropFilepath
        {
            get => issuedByCropFilepath;
            set => issuedByCropFilepath = value;
        }

        public string BirthplaceCropFilepath
        {
            get => birthplaceCropFilepath;
            set => birthplaceCropFilepath = value;
        }

        public string GenderCropFilepath
        {
            get => genderCropFilepath;
            set => genderCropFilepath = value;
        }

        public string PersonalIdFilepath
        {
            get => personalIdFilepath;
            set => personalIdFilepath = value;
        }

        public string CnpCropFilepath
        {
            get => cnpCropFilepath;
            set => cnpCropFilepath = value;
        }

        public string FirstNameCropFilepath
        {
            get => firstNameCropFilepath;
            set => firstNameCropFilepath = value;
        }

        public string MiddleNameCropFilepath
        {
            get => middleNameCropFilepath;
            set => middleNameCropFilepath = value;
        }

        public string LastNameCropFilepath
        {
            get => lastNameCropFilepath;
            set => lastNameCropFilepath = value;
        }

        public AddressCropFilePaths AddressCropFilePaths
        {
            get => addressCropFilePaths;
            set => addressCropFilePaths = value;
        }
    }
}