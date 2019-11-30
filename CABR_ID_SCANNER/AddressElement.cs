using System;

namespace CABR_ID_SCANNER
{
    public class AddressElement
    {
        private string sourceFilepath;
        private string outputCropFilepath;
        private int markXPosition;
        private int markWidth;
        private int elementHeight;
        private AddressElementType elementType;

        public AddressElement(string sourceFilepath, string outputCropFilepath, int markXPosition, int markWidth,
            int elementHeight, AddressElementType elementType)
        {
            this.sourceFilepath = sourceFilepath ?? throw new ArgumentNullException(nameof(sourceFilepath));
            this.outputCropFilepath = outputCropFilepath ?? throw new ArgumentNullException(nameof(outputCropFilepath));
            this.markXPosition = markXPosition;
            this.markWidth = markWidth;
            this.elementHeight = elementHeight;
            this.elementType = elementType;
        }

        public string SourceFilepath
        {
            get => sourceFilepath;
            set => sourceFilepath = value;
        }

        public int ElementHeight
        {
            get => elementHeight;
            set => elementHeight = value;
        }

        public AddressElementType ElementType
        {
            get => elementType;
            set => elementType = value;
        }

        public int MarkXPosition
        {
            get => markXPosition;
            set => markXPosition = value;
        }

        public string OutputCropFilepath
        {
            get => outputCropFilepath;
            set => outputCropFilepath = value;
        }

        public int MarkWidth
        {
            get => markWidth;
            set => markWidth = value;
        }
    }
}