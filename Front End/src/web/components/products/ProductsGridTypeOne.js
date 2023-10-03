
import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import ProductQuickView from './ProductQuickView';
import Config from '../../../helpers/Config';
import ProductRatingStars from './ProductRatingStars';
import { AddCustomerWishList, AddProductToCart } from '../../../helpers/CartHelper';
import rootAction from '../../../stateManagment/actions/rootAction';
import { useSelector, useDispatch } from 'react-redux';
import { makeProductShortDescription, replaceWhiteSpacesWithDashSymbolInUrl } from '../../../helpers/ConversionHelper';
import { GetDefaultCurrencySymbol } from '../../../helpers/CommonHelper';


const ProductsGridTypeOne = (props) => {
    const dispatch = useDispatch();
    const navigate = useNavigate();

    const [ProductListMainClass, setProductListMainClass] = useState(props.ProductListMainClass);
    const [ProductsList, setProductsListGrid] = useState([]);
    const [adminPanelBaseURL, setBaseUrl] = useState(Config['ADMIN_BASE_URL']);


    const HandleAddToCart = (ProductID, ProductName, Price, defaultImage) => {

        let cartItems = AddProductToCart(ProductID, ProductName, Price, 0, '', 0, '', 1, defaultImage);

        // reduxStore.dispatch(rootAction.cartAction.setCustomerCart(cartItems));
        // reduxStore.dispatch(rootAction.cartAction.SetTotalCartItems(JSON.parse(cartItems).length));

        dispatch(rootAction.cartAction.setCustomerCart(cartItems));
        dispatch(rootAction.cartAction.SetTotalCartItems(JSON.parse(cartItems).length));

    }

    const reloadProductDetail = (_productId, _categoryName, _productName) => {
            
        let productDetailUrlFromForceReload = `/product-detail/${_productId}/${replaceWhiteSpacesWithDashSymbolInUrl(_categoryName) ?? "shop"}/${replaceWhiteSpacesWithDashSymbolInUrl(_productName)}`

        if (props.ProductDetailPageForceUpload != undefined && props.ProductDetailPageForceUpload != null && props.ProductDetailPageForceUpload == true && _productId != undefined) {
            navigate(productDetailUrlFromForceReload, { replace: true });
            window.location.reload();
        }
    }


    const HandleCustomerWishList = (ProductID, ProductName, Price, DiscountedPrice, DiscountId, IsDiscountCalculated, CouponCode, defaultImage) => {


        let customerWishList = AddCustomerWishList(ProductID, ProductName, Price, DiscountedPrice, DiscountId, IsDiscountCalculated, CouponCode, 0, '', 0, '', 1, defaultImage);

        //--store in storage
        localStorage.setItem("customerWishList", customerWishList);
        dispatch(rootAction.cartAction.setCustomerWishList(customerWishList));

    }



    useEffect(() => {
        setProductsListGrid(props.ProductsList);
        console.log('Products list: ');
        console.log(props.ProductsList)
    }, [props.ProductsList]);







    return (
        <>

            {
                ProductsList?.map((item, idx) =>


                    <div className={ProductListMainClass} >
                        <div className="single-product-box">
                            <div className="product-image">



                                {(() => {
                              
                                    let urlViewDetailImage = `/product-detail/${item.ProductId}/${replaceWhiteSpacesWithDashSymbolInUrl(item.CategoryName) ?? "shop"}/${replaceWhiteSpacesWithDashSymbolInUrl(item.ProductName)}`;
                                    return (
                                        <>
                                            <Link to={urlViewDetailImage} onClick={() => reloadProductDetail(item.ProductId, item.CategoryName, item.ProductName)}>

                                                {
                                                    item?.ProductImagesJson?.map((img, imgIdx) =>
                                                        <>
                                                            <img src={adminPanelBaseURL + img.AttachmentURL} alt="image" />

                                                        </>

                                                    )
                                                }

                                            </Link>
                                        </>
                                    );
                                })()}




                                <ul>

                                    <li>
                                        <Link to="#" data-tip="Add to Wishlist" data-place="left"
                                            onClick={(e) => {
                                                e.preventDefault(); HandleCustomerWishList(item.ProductId, item.ProductName, item.Price, item.DiscountedPrice, item.DiscountId, item.IsDiscountCalculated, item.CouponCode, (item?.ProductImagesJson[0]?.AttachmentURL != undefined ? item?.ProductImagesJson[0]?.AttachmentURL : ""))
                                            }}
                                        >
                                            <i className="far fa-heart"></i>
                                        </Link>
                                    </li>

                                </ul>
                            </div>

                            <div className="product-content">
                                <h3>

                                    <Link to={`/product-detail/${item.ProductId}/${replaceWhiteSpacesWithDashSymbolInUrl(item.CategoryName) ?? "shop"}/${replaceWhiteSpacesWithDashSymbolInUrl(item.ProductName)}`} >
                                        {makeProductShortDescription(item.ProductName, 45)}
                                    </Link>
                                </h3>

                                <div className="product-price">
                                    <span className="new-price">

                                        {item.DiscountedPrice != undefined && item.DiscountedPrice > 0 ?
                                            <>
                                                <del style={{ color: "#9494b9" }}>{GetDefaultCurrencySymbol()}{item.Price}</del> &nbsp; {GetDefaultCurrencySymbol()}{item.DiscountedPrice}
                                            </>
                                            :
                                            <>
                                                ${item.Price}
                                            </>

                                        }
                                    </span>
                                </div>


                                <ProductRatingStars Rating={item.Rating == undefined || item.Rating == null ? 5 : item.Rating} />



                                {/* <Link to="#"

                                    className="btn btn-light"
                                    onClick={(e) => {
                                        e.preventDefault(); HandleAddToCart(item.ProductId, item.ProductName, item.Price, (item?.ProductImagesJson[0]?.AttachmentURL != undefined ? item?.ProductImagesJson[0]?.AttachmentURL : ""))
                                    }}
                                >
                                    <a

                                    >
                                        Add to Cart
                                    </a>
                                </Link> */}



                                {(() => {

                                    let urlViewDetail = `/product-detail/${item.ProductId}/${replaceWhiteSpacesWithDashSymbolInUrl(item.CategoryName) ?? "shop"}/${replaceWhiteSpacesWithDashSymbolInUrl(item.ProductName)}`;
                                    return (
                                        <>
                                            <Link to={urlViewDetail}
                                                onClick={() => reloadProductDetail(item.ProductId, item.CategoryName, item.ProductName)}
                                                className="btn btn-light"
                                            >
                                                <a

                                                >
                                                    View Detail
                                                </a>
                                            </Link>
                                        </>
                                    );
                                })()}


                            </div>
                        </div>
                    </div>

                )
            }




        </>
    );

}


export default ProductsGridTypeOne;

