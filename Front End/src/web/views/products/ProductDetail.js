import React, { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import Slider from "react-slick";
import { MakeApiCallSynchronous, MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import { AddCustomerWishList, AddProductToCart } from '../../../helpers/CartHelper';
import Config from '../../../helpers/Config';
import { makePriceRoundToTwoPlaces, makeProductShortDescription, setProductDescriptionImagesUrl } from '../../../helpers/ConversionHelper';
import { showErrorMsg, showInfoMsg, showSuccessMsg, validateAnyFormField } from '../../../helpers/ValidationHelper';
import ProductDetailImages from '../../components/products/ProductDetailImages';
import ProductRatingStars from '../../components/products/ProductRatingStars';
import RelatedProducts from '../../components/products/RelatedProducts';
import BestFacilities from '../../components/shared/BestFacilities';
import SizeGuide from '../../components/shared/SizeGuide';
import { Helmet } from 'react-helmet';
import { useSelector, useDispatch } from 'react-redux';
import rootAction from '../../../stateManagment/actions/rootAction';
import { LOADER_DURATION } from '../../../helpers/Constants';
import { reduxStore } from '../../../stateManagment/reduxStore';
import { GetDefaultCurrencySymbol } from '../../../helpers/CommonHelper';
import ProductVariants from '../../components/products/ProductVariants';

const ProductDetail = () => {
    const dispatch = useDispatch();
    const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);
    const [qty, setQuantity] = useState(1);
    const [max, setMax] = useState(1);
    const [min, setMin] = useState(1);
    const [ActiveSize, setActiveSize] = useState(
        {
            SizeID: 0,
            ShortName: ""
        }
    );

    const [ActiveColor, setActiveColor] = useState(
        {
            ColorID: 0,
            ColorName: ""
        }
    );

    const [sizeGuide, setSizeGuide] = useState(false);
    const [productDetail, setProductDetail] = useState({});
    const [productReviews, setProductReviews] = useState([]);
    const [paymentMethods, setPaymentMethods] = useState([]);
    const [productAllAttributes, setProductAllAttributes] = useState([]);
    const [productSelectedAttributes, setProductSelectedAttributes] = useState([]);
    const [adminPanelBaseURL, setadminPanelBaseURL] = useState(Config['ADMIN_BASE_URL']);
    const [showProductVariantsPopup, setShowProductVariantsPopup] = useState(false);
    const [productActualPrice, setProductActualPrice] = useState(0.00);
    const [productDiscountedPrice, setProductDiscountedPrice] = useState(0.00);

    //--set review variables
    const [ReviewerName, setReviewerName] = useState('');
    const [ReviewerEmail, setReviewerEmail] = useState('');
    const [ReviewTitle, setReviewTitle] = useState('');
    const [ReviewBody, setReviewBody] = useState('');
    const [ReviewRating, setReviewRating] = useState(1);

    //--set product id from url
    const params = useParams();
    const [ProductId, setProductId] = useState(params.product_id ?? 0);




    const SubmitReviewForm = async () => {



        let isValid = false;
        let validationArray = [];

        //--validation for name
        isValid = validateAnyFormField('Name', ReviewerName, 'text', null, 200, true);
        if (isValid == false) {
            validationArray.push({
                isValid: isValid
            });
        }

        //--validation for email
        isValid = validateAnyFormField('Email', ReviewerEmail, 'email', null, 200, true);
        if (isValid == false) {
            validationArray.push({
                isValid: isValid
            });
        }

        //--validation for title of review
        isValid = validateAnyFormField('Review Title', ReviewTitle, 'text', null, 200, true);
        if (isValid == false) {
            validationArray.push({
                isValid: isValid
            });
        }

        //--validation for body of review
        isValid = validateAnyFormField('Review Body', ReviewBody, 'text', null, 200, true);
        if (isValid == false) {
            validationArray.push({
                isValid: isValid
            });
        }

        //--validation for product id of review
        isValid = validateAnyFormField('Product Id', ProductId, 'number', null, 200, true);
        if (isValid == false) {
            validationArray.push({
                isValid: isValid
            });
        }

        //--check if any field is not valid
        if (validationArray != null && validationArray.length > 0) {

            isValid = false;
            return false;
        } else {
            isValid = true;
        }


        if (isValid) {

            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }


            const param = {
                requestParameters: {
                    ProductId: ProductId,
                    ReviewerName: ReviewerName,
                    ReviewerEmail: ReviewerEmail,
                    ReviewTitle: ReviewTitle,
                    ReviewBody: ReviewBody,
                    ReviewRating: ReviewRating,
                },
            };


            //--make api call for saving review data
            const response = await MakeApiCallAsync(Config.END_POINT_NAMES['INSERT_PRODUCT_REVIEW'], null, param, headers, "POST", true);
            if (response != null && response.data != null) {
                let detail = JSON.parse(response.data.data);
                if (detail[0].ResponseMsg == "Saved Successfully") {
                    showSuccessMsg("Your review submitted successfully!");

                    //--Empty form
                    await setReviewerName("");
                    await setReviewerEmail("");
                    await setReviewTitle("");
                    await setReviewBody("");

                } else {
                    showErrorMsg("An error occured. Please try again later!");
                }
            }
        }


    }

    const useTagFunc = () => {

        let useTag = '<use xlink:href="#star" />';
        return <svg className="star" dangerouslySetInnerHTML={{ __html: useTag }} />;
    }


    const DecreaseItem = () => {

        if (qty > 1) {
            setQuantity((qty) - 1);
        }
    }

    const IncrementItem = () => {
        if (productDetail.OrderMaximumQuantity != undefined && productDetail.OrderMaximumQuantity != null && productDetail.OrderMaximumQuantity > 0) {
            if ((qty + 1) > productDetail.OrderMaximumQuantity) {
                showErrorMsg(`Can not add more than ${productDetail.OrderMaximumQuantity} for this product`);
            } else {
                setQuantity((qty) + 1);
            }
        } else {
            if (qty < 10) {
                setQuantity((qty) + 1);
            }
        }

    }

    const openTabSection = (evt, tabNmae) => {
        let i, tabcontent, tablinks;
        tabcontent = document.getElementsByClassName("tabs_item");
        for (i = 0; i < tabcontent.length; i++) {
            tabcontent[i].classList.remove("fadeInUp");
            tabcontent[i].style.display = "none";
        }

        tablinks = document.getElementsByTagName("li");
        for (i = 0; i < tablinks.length; i++) {
            tablinks[i].className = tablinks[i].className.replace("current", "");
        }

        document.getElementById(tabNmae).style.display = "block";
        document.getElementById(tabNmae).className += " fadeInUp animated";
        evt.currentTarget.className += "current";
    }

    const openSizeGuide = () => {
        setSizeGuide(true);
    }

    const closeSizeGuide = () => {
        setSizeGuide(false);
    }

    const openProductVariants = () => {
        setShowProductVariantsPopup(true);
    }

    const closeProductVariantPopup = () => {
        setShowProductVariantsPopup(false);
    }

    const setProductVariantsFromPopup = (PrimaryKeyValue, ProductAttributeID) => {

        let tempProdAttr = [];
        tempProdAttr = productSelectedAttributes;

        let isAttributeExists = tempProdAttr?.find(x => x.ProductAttributeID == ProductAttributeID);

        //--If attribute already exists then just update its value
        if (isAttributeExists != null && isAttributeExists != undefined && isAttributeExists.ProductAttributeID > 0) {
            let objIndex = tempProdAttr.findIndex((obj => obj.ProductAttributeID == ProductAttributeID));
            tempProdAttr[objIndex].PrimaryKeyValue = PrimaryKeyValue;

        } else {
            tempProdAttr.push({
                ProductId: ProductId,
                ProductAttributeID: ProductAttributeID,
                PrimaryKeyValue: PrimaryKeyValue,

            });
        }

        //--Set in product selected attributes
        setProductSelectedAttributes(tempProdAttr);




        //--Set any extra price if associated with this attribute
        let additionalPrice = 0;
        for (let index = 0; index < tempProdAttr.length; index++) {
            let priceData = productAllAttributes?.find(x => x.ProductAttributeID == tempProdAttr[index].ProductAttributeID && x.PrimaryKeyValue == tempProdAttr[index].PrimaryKeyValue);
            if (priceData != null && priceData != undefined && priceData.AdditionalPrice != undefined && priceData.AdditionalPrice > 0) {
                additionalPrice = additionalPrice + priceData.AdditionalPrice;
            }

        }


        //--Set product actual price
        setProductActualPrice(makePriceRoundToTwoPlaces(productDetail.Price + additionalPrice));

        //--Set product discounted price
        setProductDiscountedPrice(makePriceRoundToTwoPlaces(productDetail.DiscountedPrice + additionalPrice));


        console.log(productSelectedAttributes);
    }


    const HandleAddToCart = () => {


        //--check if size selected
        if (productDetail?.ProductSizesJson?.length != undefined && productDetail?.ProductSizesJson?.length > 0) {
            if (ActiveSize.SizeID == undefined || ActiveSize.SizeID < 1) {
                showInfoMsg("Select size of product!");
                return false;
            }
        }

        //--check if color selected
        if (productDetail?.ProductColorsJson.length != undefined && productDetail?.ProductColorsJson.length > 0) {
            if (ActiveColor.ColorID == undefined || ActiveColor.ColorID < 1) {
                showInfoMsg("Select color of product!");
                return false;
            }
        }

        //--validate all others attributes except color and size because its already validated above
        let localAttributes = productAllAttributes?.filter(x => x.ProductAttributeID != Config.PRODUCT_ATTRIBUTE_ENUM['Color'] && x.ProductAttributeID != Config.PRODUCT_ATTRIBUTE_ENUM['Size']);
        for (let index = 0; index < localAttributes.length; index++) {
            const elementAttr = localAttributes[index];
            if (elementAttr?.IsRequiredAttribute != undefined && elementAttr?.IsRequiredAttribute == true) {
                if (!productSelectedAttributes.some(x => x.ProductAttributeID === elementAttr.ProductAttributeID)) {
                    showInfoMsg("Please select " + elementAttr.AttributeDisplayName + " variant!");
                    return false;
                }
            }
        }

        //--check if quantity selected
        if (qty == undefined || qty < 1) {
            showInfoMsg("Select quantity!");
            return false;
        }

        let defaultImage = (productDetail?.ProductImagesJson?.length > 0) ? productDetail.ProductImagesJson[0].AttachmentURL : '';
        let cartItems = AddProductToCart(ProductId,  qty, productSelectedAttributes, defaultImage);

        // reduxStore.dispatch(rootAction.cartAction.setCustomerCart(cartItems));
        // reduxStore.dispatch(rootAction.cartAction.SetTotalCartItems(JSON.parse(cartItems).length));

        dispatch(rootAction.cartAction.setCustomerCart(cartItems));
        dispatch(rootAction.cartAction.SetTotalCartItems(JSON.parse(cartItems).length));


    }


    const HandleCustomerWishList = () => {

        let defaultImageWishList = (productDetail?.ProductImagesJson?.length > 0) ? productDetail.ProductImagesJson[0].AttachmentURL : '';
        let customerWishList = AddCustomerWishList(ProductId, productDetail.ProductName, productDetail.Price, productDetail.DiscountedPrice, productDetail.DiscountId, productDetail.IsDiscountCalculated, productDetail.CouponCode, ActiveSize.SizeID, ActiveSize.ShortName, 0, '', qty, defaultImageWishList);

        //--store in storage
        localStorage.setItem("customerWishList", customerWishList);
        dispatch(rootAction.cartAction.setCustomerWishList(customerWishList));

    }


    useEffect(() => {
        // declare the data fetching function
        const getProductDetail = async () => {

            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }


            const param = {
                requestParameters: {
                    ProductId: ProductId,
                    recordValueJson: "[]",
                },
            };



            //--Get product detail
            const response = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_PRODUCT_DETAIL'], null, param, headers, "POST", true);
            if (response != null && response.data != null) {
                let detail = JSON.parse(response.data.data);
                console.log("Product detail: ");
                console.log(detail);
                await setProductDetail(detail[0]);

                //--Set product actual price
                await setProductActualPrice(detail[0].Price);

                //--Set product discounted price
                await setProductDiscountedPrice(detail[0].DiscountedPrice);

            }



            //--Get product reviews
            const responseReviews = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_PRODUCT_REVIEWS'], null, param, headers, "POST", true);
            if (responseReviews != null && responseReviews.data != null) {
                await setProductReviews(JSON.parse(responseReviews.data.data));

            }

            //--Get payment methods
            const responsePaymentMethods = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_PAYMENT_METHODS'], null, param, headers, "POST", true);
            if (responsePaymentMethods != null && responsePaymentMethods.data != null) {
                await setPaymentMethods(JSON.parse(responsePaymentMethods.data.data));

            }


        }

        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));

        // call the function
        getProductDetail().catch(console.error);

        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);

        //--scroll page top top becuase the product detail page giving issue
        setTimeout(() => {
            window.scrollTo({ top: 0, left: 0, behavior: 'smooth' });
        }, 500);


    }, [])



    return (
        <>
            <Helmet>
                <title>{siteTitle} Product Detail - {productDetail?.MetaTitle != undefined ? productDetail?.MetaTitle : ""}</title>
                <meta name="description" content={siteTitle + " - " + productDetail?.MetaDescription != undefined ? productDetail?.MetaDescription : "product description"} />
                <meta name="keywords" content={productDetail?.MetaKeywords != undefined ? productDetail?.MetaKeywords : "product description"}></meta>
            </Helmet>

            <section className="products-details-area pt-60">
                <div className="container">
                    <div className="row">

                        {
                            productDetail?.ProductImagesJson?.length > 0 ?
                                <ProductDetailImages
                                    ProductImages={productDetail.ProductImagesJson}
                                />
                                :
                                <>
                                </>
                        }


                        <div className="col-lg-6 col-md-6">
                            <div className="product-details-content">
                                <h3>{productDetail.ProductName}</h3>
                                <div className="price">
                                    <span className="new-price">


                                        {productDiscountedPrice != undefined && productDiscountedPrice > 0 ?
                                            <>
                                                <del style={{ color: "#9494b9" }}>{GetDefaultCurrencySymbol()}{productActualPrice}</del> &nbsp; {GetDefaultCurrencySymbol()}{productDiscountedPrice}
                                            </>
                                            :
                                            <>
                                                {GetDefaultCurrencySymbol()}{productActualPrice}
                                            </>

                                        }
                                    </span>
                                </div>

                                <div className="product-review">

                                    {
                                        productDetail.Rating != undefined && productDetail.Rating != null ?
                                            <>
                                                <ProductRatingStars Rating={productDetail.Rating} />
                                                <Link to="#" className="rating-count">
                                                    {productDetail.TotalReviews} reviews
                                                </Link>
                                            </>
                                            :
                                            <>
                                            </>
                                    }



                                </div>

                                <p>


                                    {
                                        makeProductShortDescription(productDetail.ShortDescription, 103)
                                    }


                                </p>

                                <ul className="product-info">
                                    <li><span>Vendor:</span> <Link to="#">{productDetail.VendorName}</Link></li>


                                    {(() => {

                                        if (productDetail.DisplayStockQuantity != undefined && productDetail.DisplayStockQuantity == true) {

                                            if (productDetail.StockQuantity != null && productDetail.StockQuantity != undefined && productDetail.StockQuantity > 0) {
                                                return (
                                                    <>
                                                        <li><span>Availability:</span> <Link to="#">In stock ({productDetail.StockQuantity} items)</Link></li>
                                                    </>
                                                );
                                            } else {
                                                return (
                                                    <>
                                                        <li><span>Availability:</span> <Link to="#">Out of stock</Link></li>
                                                    </>
                                                );
                                            }


                                        }

                                    })()}



                                    <li><span>Brand:</span> <Link to="#">{productDetail.ManufacturerName}</Link></li>
                                </ul>


                                {
                                    productDetail != undefined && productDetail.ProductColorsJson != undefined && productDetail.ProductColorsJson.length > 0
                                        ?
                                        <div className="product-color-switch">
                                            <h4>Color:</h4>

                                            <div className="">

                                                {
                                                    productDetail?.ProductColorsJson?.map((item, idx) =>


                                                        <span key={idx}
                                                            className={(ActiveColor.ColorID === item.ColorID) ? "product-color-cell  product-color-cell-active" : "product-color-cell"}
                                                            style={{ backgroundColor: `${item.HexCode}` }}
                                                            onClick={(e) => {
                                                                e.preventDefault();
                                                                setActiveColor(
                                                                    {
                                                                        ColorID: item.ColorID,
                                                                        ColorName: item.ColorName
                                                                    }
                                                                );
                                                                setProductVariantsFromPopup(item.ColorID, Config.PRODUCT_ATTRIBUTE_ENUM['Color']);
                                                            }}
                                                        >

                                                        </span>

                                                    )}




                                            </div>

                                        </div>
                                        :
                                        <>

                                        </>
                                }







                                {
                                    productDetail != undefined && productDetail.ProductSizesJson != undefined && productDetail.ProductSizesJson.length > 0
                                        ?
                                        <div className="product-size-wrapper">
                                            <h4>Size:</h4>

                                            <ul>


                                                {
                                                    productDetail?.ProductSizesJson?.map((item, idx) =>


                                                        <li key={idx}
                                                            className={(ActiveSize.SizeID === item.SizeID) ? "active" : null}
                                                        >
                                                            <Link to="#"

                                                                onClick={(e) => {
                                                                    e.preventDefault();
                                                                    setActiveSize(
                                                                        {
                                                                            SizeID: item.SizeID,
                                                                            ShortName: item.ShortName
                                                                        }
                                                                    );
                                                                    setProductVariantsFromPopup(item.SizeID, Config.PRODUCT_ATTRIBUTE_ENUM['Size']);
                                                                }}

                                                            >
                                                                {item.ShortName}
                                                            </Link>
                                                        </li>

                                                    )}


                                            </ul>
                                        </div>

                                        :
                                        <>
                                        </>
                                }


                                <div className="product-info-btn">


                                    {
                                        productAllAttributes != undefined &&
                                            productAllAttributes?.filter(x => x.ProductAttributeID != Config.PRODUCT_ATTRIBUTE_ENUM['Color']
                                                && x.ProductAttributeID != Config.PRODUCT_ATTRIBUTE_ENUM['Size']).length > 0
                                            ?

                                            <Link to="#"
                                                onClick={(e) => {
                                                    e.preventDefault();
                                                    openProductVariants();
                                                }}
                                                className="product-var-product-btn"

                                            >
                                                <i className="fas fa-list"></i>Product Variants
                                               
                                            </Link>
                                            :
                                            <>
                                            </>
                                    }





                                    <Link to="#"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            openSizeGuide();
                                        }}

                                    >
                                        <i className="fas fa-crop"></i> Size guide
                                    </Link>
                                    {/* <Link to="#">
                                        <i className="fas fa-truck"></i> Shipping
                                    </Link> */}

                                    <Link to="/contact-us">
                                        <i className="far fa-envelope"></i> Ask about this product
                                    </Link>
                                </div>

                                <div className="product-add-to-cart">
                                    <div className="input-counter">
                                        <span
                                            className="minus-btn"
                                            onClick={DecreaseItem}
                                        >
                                            <i className="fas fa-minus"></i>
                                        </span>
                                        <input
                                            type="text"
                                            value={qty}
                                            min={min}
                                            max={max}
                                            onChange={(e) => setQuantity(e.target.value)}

                                        />
                                        <span
                                            className="plus-btn"
                                            onClick={IncrementItem}
                                        >
                                            <i className="fas fa-plus"></i>
                                        </span>
                                    </div>

                                    <button
                                        type="button"
                                        className="btn btn-primary"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            HandleAddToCart();
                                        }}

                                    >
                                        <i className="fas fa-cart-plus"></i> Add to Cart
                                    </button>
                                </div>

                                <div className="wishlist-compare-btn">
                                    <Link to="#" className="btn btn-light"
                                        onClick={(e) => {
                                            e.preventDefault();
                                            HandleCustomerWishList();
                                        }}
                                    >
                                        <i className="far fa-heart"></i> Add to Wishlist
                                    </Link>

                                    {/* <Link to="#" className="btn btn-light">
                                        <i className="fas fa-balance-scale"></i> Add to Compare
                                    </Link> */}
                                </div>

                                {/* <div className="buy-checkbox-btn">
                                    <div className="item">
                                        <input className="inp-cbx" id="cbx" type="checkbox" />
                                        <label className="cbx" htmlFor="cbx">
                                            <span>
                                                <svg width="12px" height="10px" viewBox="0 0 12 10">
                                                    <polyline points="1.5 6 4.5 9 10.5 1"></polyline>
                                                </svg>
                                            </span>
                                            <span>I agree with the terms and conditions</span>
                                        </label>
                                    </div>

                                    <div className="item">
                                        <Link to="#" className="btn btn-primary">
                                            Buy it now!
                                        </Link>
                                    </div>
                                </div> */}

                                <div className="custom-payment-options">
                                    <span>Guaranteed safe checkout:</span>

                                    <div className="payment-methods">





                                        {
                                            paymentMethods?.map((item, idx) =>

                                                <Link to="#">
                                                    <img src={adminPanelBaseURL + item.ImageUrl} alt="image" />
                                                </Link>

                                            )}






                                    </div>
                                </div>
                            </div>
                        </div>


                        {/* Product Tabs */}
                        <div className="col-lg-12 col-md-12 mt-3">
                            <div className="tab products-details-tab">
                                <div className="row">
                                    <div className="col-lg-12 col-md-12">
                                        <ul className="tabs">
                                            <li
                                                onClick={(e) => { e.preventDefault(); openTabSection(e, 'tab1') }}
                                                className="current"
                                            >
                                                <span className="tabs-nav-text">
                                                    <div className="dot"></div> Description
                                                </span>
                                            </li>

                                            <li onClick={(e) => { e.preventDefault(); openTabSection(e, 'tab2') }}>
                                                <span className="tabs-nav-text">
                                                    <div className="dot"></div> Additional information
                                                </span>
                                            </li>

                                            <li onClick={(e) => { e.preventDefault(); openTabSection(e, 'tab3') }}>
                                                <span className="tabs-nav-text">
                                                    <div className="dot"></div> Shipping
                                                </span>
                                            </li>

                                            <li onClick={(e) => { e.preventDefault(); openTabSection(e, 'tab4') }}>
                                                <span className="tabs-nav-text">
                                                    <div className="dot"></div> Why Buy From Us
                                                </span>
                                            </li>

                                            <li onClick={(e) => { e.preventDefault(); openTabSection(e, 'tab5') }}>
                                                <span className="tabs-nav-text">
                                                    <div className="dot"></div> Reviews
                                                </span>
                                            </li>
                                        </ul>
                                    </div>

                                    <div className="col-lg-12 col-md-12">
                                        <div className="tab_content">
                                            <div id="tab1" className="tabs_item">
                                                <div className="products-details-tab-content">

                                                    {
                                                        productDetail.FullDescription != undefined ?
                                                            <div dangerouslySetInnerHTML={{ __html: setProductDescriptionImagesUrl(productDetail.FullDescription)  }}>
                                                            </div>
                                                            :
                                                            <>
                                                            </>
                                                    }

                                                </div>
                                            </div>

                                            <div id="tab2" className="tabs_item">
                                                <div className="products-details-tab-content">
                                                    <div className="table-responsive">
                                                        <table className="table table-striped">
                                                            <tbody>
                                                                <tr>
                                                                    <td>Color:</td>
                                                                    <td>
                                                                        Red, white , blue

                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td>Tags:</td>
                                                                    <td>

                                                                        <ul>


                                                                            {
                                                                                productDetail?.ProductTagsJson?.map((item, idx) =>


                                                                                    <li key={idx}>
                                                                                        {item.TagName}
                                                                                    </li>

                                                                                )}


                                                                        </ul>



                                                                    </td>
                                                                </tr>


                                                            </tbody>
                                                        </table>
                                                    </div>
                                                </div>
                                            </div>

                                            <div id="tab3" className="tabs_item">
                                                <div className="products-details-tab-content">
                                                    <div className="table-responsive">
                                                        <table className="table table-bordered">
                                                            <tbody>
                                                                <tr>
                                                                    <td>Shipping Free</td>
                                                                    <td>{productDetail?.IsShippingFree == true ? "Yes" : "No"}</td>
                                                                </tr>

                                                                <tr>
                                                                    <td>Delivery Methods</td>
                                                                    <td>
                                                                        <ul>


                                                                            {
                                                                                productDetail?.ProductShipMethodsJson?.map((item, idx) =>


                                                                                    <li key={idx}>
                                                                                        {item.ShippingMethodName}
                                                                                    </li>

                                                                                )}


                                                                        </ul>
                                                                    </td>
                                                                </tr>

                                                                <tr>
                                                                    <td>Estimated Shipping Days</td>
                                                                    <td>
                                                                        {productDetail.EstimatedShippingDays}
                                                                    </td>
                                                                </tr>
                                                            </tbody>
                                                        </table>
                                                    </div>
                                                </div>
                                            </div>

                                            <div id="tab4" className="tabs_item">
                                                <div className="products-details-tab-content">
                                                    <p>Here are 5 more great reasons to buy from us:</p>

                                                    <ol>
                                                        <li>Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.</li>
                                                        <li> Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.</li>
                                                        <li>when an unknown printer took a galley of type and scrambled it to make a type specimen book.</li>
                                                        <li>Lorem Ipsum is simply dummy text of the printing and typesetting industry.</li>
                                                        <li>when an unknown printer took a galley of type and scrambled it to make a type specimen book.</li>
                                                    </ol>
                                                </div>
                                            </div>

                                            <div id="tab5" className="tabs_item">
                                                <div className="products-details-tab-content">
                                                    <div className="product-review-form">
                                                        <h3>Customer Reviews</h3>



                                                        <div className="review-comments">



                                                            {
                                                                productReviews?.map((item, idx) =>

                                                                    <div className="review-item">


                                                                        <ProductRatingStars Rating={item.Rating == undefined || item.Rating == null ? 5 : item.Rating} />


                                                                        <h3>{item.Title}</h3>
                                                                        <span><strong>{item.ReviewerName}</strong> on <strong>{item.ReviewDate}</strong></span>
                                                                        <p>
                                                                            {
                                                                                makeProductShortDescription(item.Body, 500)
                                                                            }
                                                                        </p>

                                                                        {/* <Link to="#" className="review-report-link">
                                                                            Report as Inappropriate
                                                                        </Link> */}
                                                                    </div>

                                                                )}



                                                        </div>

                                                        <div className="review-form">
                                                            <h3>Write a Review</h3>

                                                            <form>
                                                                <div className="form-group">
                                                                    <label>Name</label>
                                                                    <input type="text" id="name" name="name" placeholder="Enter your name" className="form-control"
                                                                        value={ReviewerName}
                                                                        onChange={(e) => setReviewerName(e.target.value)}
                                                                    />
                                                                </div>

                                                                <div className="form-group">
                                                                    <label>Email</label>
                                                                    <input type="email" id="email" name="email" placeholder="Enter your email" className="form-control"
                                                                        value={ReviewerEmail}
                                                                        onChange={(e) => setReviewerEmail(e.target.value)}
                                                                    />
                                                                </div>

                                                                <div className="review-rating">
                                                                    <p>Rate this item</p>

                                                                    <div className="star-source">
                                                                        <svg>
                                                                            <linearGradient x1="50%" y1="5.41294643%" x2="87.5527344%" y2="65.4921875%" id="grad">
                                                                                <stop stopColor="#f2b01e" offset="0%"></stop>
                                                                                <stop stopColor="#f2b01e" offset="60%"></stop>
                                                                                <stop stopColor="#f2b01e" offset="100%"></stop>
                                                                            </linearGradient>
                                                                            <symbol id="star" viewBox="153 89 106 108">
                                                                                <polygon id="star-shape" stroke="url(#grad)" strokeWidth="5" fill="currentColor" points="206 162.5 176.610737 185.45085 189.356511 150.407797 158.447174 129.54915 195.713758 130.842203 206 95 216.286242 130.842203 253.552826 129.54915 222.643489 150.407797 235.389263 185.45085"></polygon>
                                                                            </symbol>
                                                                        </svg>
                                                                    </div>

                                                                    <div className="star-rating">
                                                                        <input type="radio" name="star" id="five"
                                                                            onChange={(e) => setReviewRating(5)}
                                                                        />
                                                                        <label htmlFor="five">
                                                                            {useTagFunc()}
                                                                        </label>

                                                                        <input type="radio" name="star" id="four"
                                                                            onChange={(e) => setReviewRating(4)}
                                                                        />
                                                                        <label htmlFor="four">
                                                                            {useTagFunc()}
                                                                        </label>

                                                                        <input type="radio" name="star" id="three"
                                                                            onChange={(e) => setReviewRating(3)}
                                                                        />
                                                                        <label htmlFor="three">
                                                                            {useTagFunc()}
                                                                        </label>

                                                                        <input type="radio" name="star" id="two"
                                                                            onChange={(e) => setReviewRating(2)}
                                                                        />
                                                                        <label htmlFor="two">
                                                                            {useTagFunc()}
                                                                        </label>

                                                                        <input type="radio" name="star" id="one"
                                                                            onChange={(e) => setReviewRating(1)}
                                                                        />
                                                                        <label htmlFor="one">
                                                                            {useTagFunc()}
                                                                        </label>
                                                                    </div>
                                                                </div>

                                                                <div className="form-group">
                                                                    <label>Review Title</label>
                                                                    <input type="text" id="review-title" name="review-title" placeholder="Enter your review a title" className="form-control"
                                                                        value={ReviewTitle}
                                                                        onChange={(e) => setReviewTitle(e.target.value)}
                                                                    />
                                                                </div>

                                                                <div className="form-group">
                                                                    <label>Body of Review (1000)</label>
                                                                    <textarea name="review-body" id="review-body" cols="30" rows="10" placeholder="Write your comments here" className="form-control"
                                                                        value={ReviewBody}
                                                                        onChange={(e) => setReviewBody(e.target.value)}
                                                                    />
                                                                </div>
                                                                <button type="button" className="btn btn-light"
                                                                    onClick={() => SubmitReviewForm()}
                                                                >Submit Review</button>
                                                            </form>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                    </div>
                </div>
            </section>


            <RelatedProducts
                ProductId={ProductId}
            />

            <BestFacilities />

            {sizeGuide ? <SizeGuide
                closeSizeGuide={closeSizeGuide}
            /> : ''}


            <ProductVariants
                ProductId={ProductId}
                showProductVariantsPopup={showProductVariantsPopup}
                closeProductVariantPopup={closeProductVariantPopup}
                setProductVariantsFromPopup={setProductVariantsFromPopup}
                productAllAttributes={productAllAttributes}
                setProductAllAttributes={setProductAllAttributes}
            />


        </>
    );

}

export default ProductDetail;
