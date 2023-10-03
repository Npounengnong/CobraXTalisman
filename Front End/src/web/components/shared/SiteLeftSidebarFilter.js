import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { MakeApiCallSynchronous, MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import Config from '../../../helpers/Config';
import "../../resources/themeContent/images/img2.jpg"

import { useSelector, useDispatch } from 'react-redux';
import rootAction from '../../../stateManagment/actions/rootAction';
import { LOADER_DURATION } from '../../../helpers/Constants';
import { makeProductShortDescription, replaceWhiteSpacesWithDashSymbolInUrl } from '../../../helpers/ConversionHelper';
import { GetDefaultCurrencySymbol} from '../../../helpers/CommonHelper';


export const SiteLeftSidebarFilter = (props) => {
    const dispatch = useDispatch();
    const [RowColCssCls, setRowColClass] = useState(props.RowColCssCls);
    const [currentSelection, setcurrentSelection] = useState(false);
    const [collection, setCollection] = useState(false);
    const [brand, setBrand] = useState(false);
    const [size, setSize] = useState(false);
    const [price, setPrice] = useState(false);
    const [color, setColor] = useState(false);
    const [tag, setTag] = useState(false);
    const [rating, setRating] = useState(false);
    const [SizeList, setSizeList] = useState([]);
    const [CategoriesList, setCategoriesList] = useState([]);
    const [ManufacturerList, setManufacturerList] = useState([]);
    const [TagsList, setTagsList] = useState([]);



    const [PriceValuesArray, setPriceValuesArray] = useState(
        [
            {
                id: "10-100",
                name: "$10 - $100"
            },
            {
                id: "100-200",
                name: " $100 - $200"
            },
            {
                id: "200-300",
                name: "$200 - $300"
            },
            {
                id: "300-400",
                name: "$300 - $400"
            },
            {
                id: "400-500",
                name: "$400 - $500"
            },
            {
                id: "500-600",
                name: "$500 - $600"
            },
            {
                id: "600-1000000000",
                name: "Above $600"
            }
        ]
    );

    //--active item index area starts here
    const [activeItemCategoryIndex, setactiveItemCategoryIndex] = useState(0);
    const [activeItemManufacturerIndex, setActiveItemManufacturerIndex] = useState(0);
    const [activeItemSizeIndex, setActiveItemSizeIndex] = useState(0);
    const [activeItemTagIndex, setActiveItemTagIndex] = useState(0);
    const [activeItemPriceIndex, setActiveItemPriceIndex] = useState(0);
    //--active item index area ends here



    const handleToggle = (e, evt) => {

        e.preventDefault();

        if (evt == "currentSelection") {
            setcurrentSelection(!currentSelection);
        } else if (evt == "collection") {
            setCollection(!collection);

        } else if (evt == "brand") {
            setBrand(!brand);
        } else if (evt == "size") {
            setSize(!size);
        } else if (evt == "price") {
            setPrice(!price);
        } else if (evt == "color") {
            setColor(!color);
        }
        else if (evt == "rating") {
            setRating(!rating);
        } else if (evt == "tag") {
            setTag(!tag);
        }
    }



    useEffect(() => {

        const GetFiltersAllValues = async () => {

             
            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
                  
            }


            const param = {
                requestParameters: {
                    PageNo: 1,
                    PageSize: 100,
                    recordValueJson: "[]",
                },
            };

            //--Get categories list
            const categoriesResponse = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_CATEGORIES_LIST'], null, param, headers, "POST", true);
            if (categoriesResponse != null && categoriesResponse.data != null) {
                await setCategoriesList(JSON.parse(categoriesResponse.data.data));
                console.log(JSON.parse(categoriesResponse.data.data))

            }

            //--Get sizes list
            const sizeResponse = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_SIZE_LIST'], null, param, headers, "POST", true);
            if (sizeResponse != null && sizeResponse.data != null) {
                await setSizeList(JSON.parse(sizeResponse.data.data));
            }

            //--Get manufacturer list
            const manufacturerResponse = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_MANUFACTURER_LIST'], null, param, headers, "POST", true);
            if (manufacturerResponse != null && manufacturerResponse.data != null) {
                await setManufacturerList(JSON.parse(manufacturerResponse.data.data));

            }

            //--Get popular tags
            const tagsResponse = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_TAGS_LIST'], null, param, headers, "POST", true);
            if (tagsResponse != null && tagsResponse.data != null) {
                await setTagsList(JSON.parse(tagsResponse.data.data));

            }



        }

        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));

        // call the function
        GetFiltersAllValues().catch(console.error);

        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);
    }, [])



    return (
        <>

            <div className={RowColCssCls}>
                <div className="woocommerce-sidebar-area">


                    <div
                        className={`collapse-widget collections-list-widget ${collection ? '' : 'open'}`}
                    >
                        <h3
                            className={`collapse-widget-title ${collection ? '' : 'active'}`}

                            onClick={(e) => { handleToggle(e, "collection"); }}
                        >
                            Categories

                            <i className="fas fa-angle-up"></i>
                        </h3>

                        <ul className={`collections-list-row ${collection ? 'block' : 'none'}`}>

                            {CategoriesList?.map((item, idx) => {

                                if (CategoriesList.filter(obj => obj.ParentCategoryID == item.CategoryID).length > 0) {
                                    return (
                                        <>
                                            <li >

                                                <h6 className="">{item.Name}</h6>

                                                <ul className="" style={{ listStyle: "none" }}>

                                                    {CategoriesList.filter(obj => obj.ParentCategoryID == item.CategoryID).map((elementChild, idxChild) => {

                                                        return (
                                                            <>
                                                                <li
                                                                    className={(activeItemCategoryIndex === elementChild.CategoryID) ? "active" : null}
                                                                >
                                                                    <Link to="#"
                                                                        onClick={(e) => {
                                                                            props.setFilterValueInParent(e, elementChild.CategoryID, "category");
                                                                            setactiveItemCategoryIndex(elementChild.CategoryID);
                                                                        }}
                                                                    >
                                                                        {elementChild.Name}
                                                                    </Link>
                                                                </li>

                                                            </>
                                                        );
                                                    })}


                                                </ul>
                                            </li>
                                        </>
                                    );
                                } else {
                                    return null;
                                }


                            })}



                        </ul>
                    </div>


                    <div
                        className={`collapse-widget brands-list-widget ${brand ? '' : 'open'}`}
                    >
                        <h3
                            className={`collapse-widget-title ${brand ? '' : 'active'}`}
                            onClick={(e) => { handleToggle(e, "brand"); }}

                        >
                            Brands

                            <i className="fas fa-angle-up"></i>
                        </h3>

                        <ul className={`brands-list-row ${brand ? 'block' : 'none'}`}>



                            {
                                ManufacturerList?.map((item, idx) =>

                                    <li
                                        className={(activeItemManufacturerIndex === item.ManufacturerID) ? "active" : null}
                                    >
                                        <Link to="#"
                                            onClick={(e) => {
                                                props.setFilterValueInParent(e, item.ManufacturerID, "brand");
                                                setActiveItemManufacturerIndex(item.ManufacturerID);
                                            }}
                                        >
                                            {item.Name}
                                        </Link>
                                    </li>

                                )}



                        </ul>
                    </div>


                    <div
                        className={`collapse-widget size-list-widget ${size ? '' : 'open'}`}
                    >
                        <h3
                            className={`collapse-widget-title ${size ? '' : 'active'}`}
                            onClick={(e) => { handleToggle(e, "size"); }}
                        >
                            Size

                            <i className="fas fa-angle-up"></i>
                        </h3>

                        <ul className={`size-list-row ${size ? 'block' : 'none'}`}>

                            {
                                SizeList?.map((item, idx) =>

                                    <li
                                        className={(activeItemSizeIndex === item.SizeID) ? "active" : null}
                                    >
                                        <Link to="#"
                                            onClick={(e) => {
                                                props.setFilterValueInParent(e, item.SizeID, "size");
                                                setActiveItemSizeIndex(item.SizeID);
                                            }}
                                        >
                                            {item.ShortName}
                                        </Link>
                                    </li>

                                )}



                        </ul>
                    </div>

                    <div
                        className={`collapse-widget price-list-widget ${price ? '' : 'open'}`}
                    >
                        <h3
                            className={`collapse-widget-title ${price ? '' : 'active'}`}
                            onClick={(e) => { handleToggle(e, "price"); }}
                        >
                            Price

                            <i className="fas fa-angle-up"></i>
                        </h3>

                        <ul className={`price-list-row ${price ? 'block' : 'none'}`}>


                            {
                                PriceValuesArray?.map((item, idx) =>

                                    <li
                                        className={(activeItemPriceIndex === item.id) ? "active" : null}
                                    >
                                        <Link to="#"
                                            onClick={(e) => {
                                                props.setFilterValueInParent(e, item.id, "price");
                                                setActiveItemPriceIndex(item.id);
                                            }}
                                        >
                                            {item.name}
                                        </Link>
                                    </li>

                                )}





                        </ul>
                    </div>


                    <div
                        className={`collapse-widget price-list-widget ${rating ? '' : 'open'}`}
                    >
                        <h3
                            className={`collapse-widget-title ${rating ? '' : 'active'}`}
                            onClick={(e) => { handleToggle(e, "rating"); }}
                        >
                            Rating

                            <i className="fas fa-angle-up"></i>
                        </h3>

                        <ul className={`price-list-row ${rating ? 'block' : 'none'}`}>
                            <li>
                                <Link to="#"
                                    onClick={(e) => { props.setFilterValueInParent(e, 5, "rating"); }}
                                >
                                    <div className="rating-side-bar">
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                    </div>
                                </Link>
                            </li>
                            <li>
                                <Link to="#"
                                    onClick={(e) => { props.setFilterValueInParent(e, 4, "rating"); }}
                                >
                                    <div className="rating-side-bar">
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="far fa-star"></i>
                                    </div>
                                </Link>
                            </li>

                            <li>
                                <Link to="#"
                                    onClick={(e) => { props.setFilterValueInParent(e, 3, "rating"); }}
                                >
                                    <div className="rating-side-bar">
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="far fa-star"></i>
                                        <i className="far fa-star"></i>
                                    </div>
                                </Link>

                            </li>

                            <li>
                                <Link to="#"
                                    onClick={(e) => { props.setFilterValueInParent(e, 2, "rating"); }}
                                >
                                    <div className="rating-side-bar">
                                        <i className="fas fa-star"></i>
                                        <i className="fas fa-star"></i>
                                        <i className="far fa-star"></i>
                                        <i className="far fa-star"></i>
                                        <i className="far fa-star"></i>
                                    </div>
                                </Link>

                            </li>

                            <li>
                                <Link to="#"
                                    onClick={(e) => { props.setFilterValueInParent(e, 1, "rating"); }}
                                >
                                    <div className="rating-side-bar">
                                        <i className="fas fa-star"></i>
                                        <i className="far fa-star"></i>
                                        <i className="far fa-star"></i>
                                        <i className="far fa-star"></i>
                                        <i className="far fa-star"></i>
                                    </div>
                                </Link>

                            </li>

                        </ul>
                    </div>




                    <div
                        className={`collapse-widget tag-list-widget ${tag ? '' : 'open'}`}
                    >
                        <h3
                            className={`collapse-widget-title ${tag ? '' : 'active'}`}
                            onClick={(e) => { handleToggle(e, "tag"); }}
                        >
                            Popular Tags

                            <i className="fas fa-angle-up"></i>
                        </h3>

                        <ul className={`tags-list-row ${tag ? 'block' : 'none'}`}>


                            {
                                TagsList?.map((item, idx) =>

                                    <li
                                        className={(activeItemTagIndex === item.TagID) ? "active" : null}
                                    >
                                        <Link to="#"
                                            onClick={(e) => {
                                                props.setFilterValueInParent(e, item.TagID, "tag");
                                                setActiveItemTagIndex(item.TagID);
                                            }}
                                        >
                                            {item.TagName}
                                        </Link>
                                    </li>

                                )}

                        </ul>
                    </div>





                    <LeftSideBarPopularProducts />



                    <div className="collapse-widget aside-trending-widget">
                        <div className="aside-trending-products">
                            <img src="/images/bestseller-hover-img1.jpg" alt="image" />

                            <div className="category">
                                <h4>Top Trending</h4>
                                <span>Spring/Summer 2018 Collection</span>
                            </div>
                            <Link to="#">

                            </Link>
                        </div>
                    </div>
                </div>
            </div>


        </>
    );

}




export const LeftSideBarPopularProducts = () => {
    const dispatch = useDispatch();

    const [PopularProductsList, setPopularProductsList] = useState([]);
    const [adminPanelBaseURL, setBaseUrl] = useState(Config['ADMIN_BASE_URL']);


    const GetPopularProductsForLeftSideBar = async () => {

        const headersPouplarProducts = {
            // customerid: userData?.UserID,
            // customeremail: userData.EmailAddress,
            Accept: 'application/json',
            'Content-Type': 'application/json',
      
        }


        const paramPouplarProducts = {
            requestParameters: {
                PageNo: 1,
                PageSize: 10,
                recordValueJson: "[]",
            },
        };

        const responsePopularProducts = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_POPULAR_PRODUCTS_LIST'], null, paramPouplarProducts, headersPouplarProducts, "POST", true);
        if (responsePopularProducts != null && responsePopularProducts.data != null) {
            await setPopularProductsList(JSON.parse(responsePopularProducts.data.data));
            console.log(JSON.parse(responsePopularProducts.data.data));
        }
    }


    useEffect(() => {

        const GetFiltersAllValues = async () => {

            //--get popular products list
            await GetPopularProductsForLeftSideBar();

        }

        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));

        // call the function
        GetFiltersAllValues().catch(console.error);

        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);
    }, [])



    return (

        <>
            <div className="collapse-widget aside-products-widget">
                <h3 className="aside-widget-title">
                    Popular Products
                </h3>



                {
                    PopularProductsList?.map((item, idx) =>

                        <div className="aside-single-products">
                            <div className="products-image">
                                <Link to="#">

                                   
                                    {
                                        item?.ProductImagesJson?.slice(0,1).map((img, imgIdx) =>
                                            <>
                                                <img src={adminPanelBaseURL + img.AttachmentURL} alt="image" />

                                            </>

                                        )
                                    }

                                </Link>
                            </div>

                            <div className="products-content">
                                {/* <span>
                                    <Link to="#">
                                        Men's
                                    </Link>
                                </span> */}
                                <h3>
                                    <Link
                                    to={`/product-detail/${item.ProductId}/${replaceWhiteSpacesWithDashSymbolInUrl(item.CategoryName) ?? "shop"}/${replaceWhiteSpacesWithDashSymbolInUrl(item.ProductName)}`}
                                    >
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
                                    {/* <span className="old-price">$291.00</span> */}
                                </div>
                            </div>
                        </div>

                    )}





            </div>

        </>

    );
}
