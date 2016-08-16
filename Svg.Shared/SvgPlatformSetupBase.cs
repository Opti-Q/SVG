using System.ComponentModel;
using Svg.DataTypes;
using Svg.FilterEffects;
using Svg.Interfaces;
using Svg.Pathing;
using Svg.Transforms;

namespace Svg
{
    public abstract class SvgPlatformSetupBase
    {
        private static readonly FileSystem FileSystem = new FileSystem();
        private static readonly SvgElementFactory ElementFactory = new SvgElementFactory();
        private static readonly SvgMarshal Marshal = new SvgMarshal();
        private static readonly SvgTypeDescriptor SvgTypeDescriptor = new SvgTypeDescriptor();
        private static readonly SvgCharComverter CharConverter = new SvgCharComverter();
        private static readonly SvgElementAttributeProvider SvgElementAttributeProvider = new SvgElementAttributeProvider();
        private static readonly DefaultLogger DefaultLogger = new DefaultLogger();
        private static readonly CultureHelper CultureHelper = new CultureHelper();
        private bool _isInitialized = false;

        public virtual void Initialize()
        {
            if (_isInitialized)
                return;

            Engine.Register<IMarshal, SvgMarshal>(() => Marshal);
            Engine.Register<ISvgTypeDescriptor, SvgTypeDescriptor>(() => SvgTypeDescriptor);
            Engine.Register<ISvgElementAttributeProvider, SvgElementAttributeProvider>(() => SvgElementAttributeProvider);
            Engine.Register<ICultureHelper, CultureHelper>(() => CultureHelper);
            Engine.Register<ILogger, DefaultLogger>(() => DefaultLogger);
            Engine.Register<ICharConverter, SvgCharComverter>(() => CharConverter);
            Engine.Register<IWebRequest, WebRequestSvc>(() => new WebRequestSvc());
            Engine.Register<IFileSystem, FileSystem>(() => FileSystem);
            Engine.Register<ISvgUnitConverter, SvgUnitConverter>(() => new SvgUnitConverter());
            Engine.Register<ISvgElementFactory, SvgElementFactory>(() => ElementFactory);

            // register enumconverters
            // see http://stackoverflow.com/questions/1999803/how-to-implement-a-typeconverter-for-a-type-and-property-i-dont-own

            TypeDescriptor.AddAttributes(typeof(SvgClipRule), new TypeConverterAttribute(typeof(SvgClipRuleConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgAspectRatio), new TypeConverterAttribute(typeof(SvgPreserveAspectRatioConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgPreserveAspectRatio), new TypeConverterAttribute(typeof(SvgPreserveAspectRatioConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgColourInterpolation), new TypeConverterAttribute(typeof(SvgColourInterpolationConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgCoordinateUnits), new TypeConverterAttribute(typeof(SvgCoordinateUnitsConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgFontStyle), new TypeConverterAttribute(typeof(SvgFontStyleConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgFontVariant), new TypeConverterAttribute(typeof(SvgFontVariantConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgFontWeight), new TypeConverterAttribute(typeof(SvgFontWeightConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgMarkerUnits), new TypeConverterAttribute(typeof(SvgMarkerUnitsConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgOrient), new TypeConverterAttribute(typeof(SvgOrientConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgOverflow), new TypeConverterAttribute(typeof(SvgOverflowConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgPointCollection), new TypeConverterAttribute(typeof(SvgPointCollectionConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgTextDecoration), new TypeConverterAttribute(typeof(SvgTextDecorationConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgTextLengthAdjust), new TypeConverterAttribute(typeof(SvgTextLengthAdjustConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgTextPathMethod), new TypeConverterAttribute(typeof(SvgTextPathMethodConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgTextPathSpacing), new TypeConverterAttribute(typeof(SvgTextPathSpacingConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgUnit), new TypeConverterAttribute(typeof(SvgUnitConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgUnitCollection), new TypeConverterAttribute(typeof(SvgUnitCollectionConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgViewBox), new TypeConverterAttribute(typeof(SvgViewBoxConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgColourMatrixType), new TypeConverterAttribute(typeof(EnumBaseConverter<SvgColourMatrixType>)));
            TypeDescriptor.AddAttributes(typeof(SvgFillRule), new TypeConverterAttribute(typeof(SvgFillRuleConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgGradientSpreadMethod), new TypeConverterAttribute(typeof(SvgGradientSpreadMethodConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgPaintServer), new TypeConverterAttribute(typeof(SvgPaintServerFactory)));
            TypeDescriptor.AddAttributes(typeof(SvgStrokeLineCap), new TypeConverterAttribute(typeof(SvgStrokeLineCapConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgStrokeLineJoin), new TypeConverterAttribute(typeof(SvgStrokeLineJoinConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgPathSegmentList), new TypeConverterAttribute(typeof(SvgPathBuilder)));
            TypeDescriptor.AddAttributes(typeof(SvgTextAnchor), new TypeConverterAttribute(typeof(SvgTextAnchorConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgTransformCollection), new TypeConverterAttribute(typeof(SvgTransformConverter)));
            TypeDescriptor.AddAttributes(typeof(SvgVisible), new TypeConverterAttribute(typeof(SvgVisibleConverter)));

            _isInitialized = true;
        }
    }
}