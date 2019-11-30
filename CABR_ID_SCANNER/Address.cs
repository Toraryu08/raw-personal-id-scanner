namespace CABR_ID_SCANNER
{
    public class Address
    {
        private string street;
        private string alley;
        private string apartment;
        private string block;
        private string district;
        private string municipality;
        private string streetNumber;
        private string city;
        private string blockEntrance;
        private string sector;

        public string Street
        {
            get => street;
            set => street = value;
        }

        public string Alley
        {
            get => alley;
            set => alley = value;
        }

        public string Apartment
        {
            get => apartment;
            set => apartment = value;
        }

        public string Block
        {
            get => block;
            set => block = value;
        }

        public string District
        {
            get => district;
            set => district = value;
        }

        public string Municipality
        {
            get => municipality;
            set => municipality = value;
        }

        public string StreetNumber
        {
            get => streetNumber;
            set => streetNumber = value;
        }

        public string City
        {
            get => city;
            set => city = value;
        }

        public string BlockEntrance
        {
            get => blockEntrance;
            set => blockEntrance = value;
        }

        public string Sector
        {
            get => sector;
            set => sector = value;
        }
        
    }
}