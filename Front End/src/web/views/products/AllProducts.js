import React, { useEffect, useState } from 'react';
import { useParams, Link, useLocation } from 'react-router-dom';
import BestFacilities from '../../components/shared/BestFacilities';
import ProductsFilterOptions from '../../components/products/ProductsFilterOptions';
import ProductsGridTypeOne from '../../components/products/ProductsGridTypeOne';
import { SiteLeftSidebarFilter } from '../../components/shared/SiteLeftSidebarFilter';
import SitePagination from '../../components/shared/SitePagination';

import SubscribeNewsLetter from '../../components/shared/SubscribeNewsLetter';
import { MakeApiCallSynchronous, MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import Config from '../../../helpers/Config';
import { useSelector, useDispatch } from 'react-redux';
import rootAction from '../../../stateManagment/actions/rootAction';
import { LOADER_DURATION } from '../../../helpers/Constants';
import { Helmet } from 'react-helmet';


const AllProducts = () => {
    const dispatch = useDispatch();
    const search = useLocation().search;
    const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);
    const [RowColCssCls, setRowColCssCls] = useState("col-lg-3 col-md-12");
    const [ProductListMainClass, setProductListMainClass] = useState("col-lg-4 col-sm-6 col-md-4 col-6 products-col-item");
    const [gridClass, setGridClass] = useState("");
    const [ProductsList, setProductsList] = useState([]);
    const [TotalRecords, setTotalRecords] = useState(0);
    const [showPagination, setshowPagination] = useState(false);
    const [PageNo, setPageNo] = useState(1);
    const [PageSize, setPageSize] = useState(9);
    const [OrderByColumnName, setOrderByColumnName] = useState('');
    

     //--set product id from url
     const params = useParams();
     const [CategoryID, setCategoryID]  = useState(params.category_id ?? 0);
 

    const [SearchTerm, setSearchTerm] = useState(new URLSearchParams(search).get('SearchTerm'));
    const [SizeID, setSizeID] = useState(null);
    const [ColorID, setColorID] = useState(null);
    const [TagID, setTagID] = useState(null);
    const [ManufacturerID, setManufacturerID] = useState(null);
    const [MinPrice, setMinPrice] = useState(null);
    const [MaxPrice, setMaxPrice] = useState(null);
    const [Rating, setRating] = useState(null);



    const handleGrid = (e) => {

        setGridClass(e);

    }


    const setFilterValueInParent = async (e, value, type) => {

        e.preventDefault();

        if (type == "category") {
            await setCategoryID(value);
            await getAllProductsAfterAnyFilterChange(1, value, null, null,null,null,null,null,null,null);

        } else if (type == "brand") {
            await setManufacturerID(value);
            await getAllProductsAfterAnyFilterChange(1 , null, value, null,null,null,null,null,null, null);
        } else if (type == "size") {

            await setSizeID(value);
            await getAllProductsAfterAnyFilterChange(1,null, null, value,null,null,null,null,null, null);

        } else if (type == "price") {

            // setTimeout(() => {
            //     const priceArray = value.split("-");
            //     setMinPrice(priceArray[0]);
            //     setMaxPrice(priceArray[1]);
            // }, 100);

            const priceArray = value.split("-");
            await setMinPrice(priceArray[0]);
            await setMaxPrice(priceArray[1]);

            await getAllProductsAfterAnyFilterChange(1,null, null, null,priceArray[0],priceArray[1],null,null,null,null);

        } else if (type == "color") {

            await setColorID(value);
            await getAllProductsAfterAnyFilterChange(1,null, null, null,null,null,null,null,value, null);

        }
        else if (type == "rating") {

            await setRating(value);
            await getAllProductsAfterAnyFilterChange(1,null, null, null,null,null,value,null,null , null);

        } else if (type == "tag") {

            await setTagID(value);
            await getAllProductsAfterAnyFilterChange(1,null, null, null,null,null,null,value,null , null);
        }


    }

    //--this function called from the ProductsFiltersOption component
    const setPageSizeFromProductFilter = async (e) => {
            
        setPageSize(parseInt(e.target.value));
        await getAllProductsAfterAnyFilterChange(1,null, null, null,null,null,null,null,null,null);

    }

    const setSortByFilter = async (e) => {
        
        setOrderByColumnName(parseInt(e.target.value));
        await getAllProductsAfterAnyFilterChange(1,null, null, null,null,null,null,null,null ,e.target.value);

    }

    //--this function called from the SitePagination component
    const setCurrentPage = async (pageNumber) => {


        setTimeout(async () => {
            await getAllProductsAfterAnyFilterChange(pageNumber , null, null, null,null,null,null,null,null, null);
        }, 200);



    }

    const getAllProductsAfterAnyFilterChange = async (pageNumber , _categoryId, _manufacturerId, _sizeId,
            _minPrice, _maxPrice, _rating, _tagId , _colorId , _orderByColumnName) => {


        try {

                
            //--start loader
            dispatch(rootAction.commonAction.setLoading(true));

            await setPageNo(pageNumber);

            let headersFromPage = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }


            let paramFromPage = {
                requestParameters: {
                    SearchTerm: SearchTerm,
                    SizeID: _sizeId ?? SizeID,
                    ColorID: _colorId ?? ColorID,
                    CategoryID: _categoryId ?? CategoryID,
                    TagID:  _tagId ?? TagID,
                    ManufacturerID: _manufacturerId ??  ManufacturerID,
                    MinPrice: _minPrice ?? MinPrice,
                    MaxPrice: _maxPrice ?? MaxPrice,
                    Rating: _rating ?? Rating,
                    OrderByColumnName:_orderByColumnName  ?? OrderByColumnName,
                    PageNo: pageNumber ?? PageNo,
                    PageSize: PageSize,
                    recordValueJson: "[]",
                },
            };

            setshowPagination(false);


            let responseAllProducts = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_All_PRODUCTS'], null, paramFromPage, headersFromPage, "POST", true);
            if (responseAllProducts != null && responseAllProducts.data != null) {

                await setProductsList(JSON.parse(responseAllProducts.data.data));
                let AllProducts = JSON.parse(responseAllProducts.data.data);
                await setTotalRecords(parseInt(AllProducts[0]?.TotalRecords ?? 0))
                console.log(JSON.parse(responseAllProducts.data.data));

                if (AllProducts.length > 0) {
                    await setshowPagination(true);
                }

            }


            //--stop loader
            setTimeout(() => {
                dispatch(rootAction.commonAction.setLoading(false));
            }, LOADER_DURATION);


        }
        catch (error) {

            //--stop loader
            setTimeout(() => {
                dispatch(rootAction.commonAction.setLoading(false));
            }, LOADER_DURATION);

        }






    }



    useEffect(() => {

        const getAllProducts = async () => {

            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            }


            const param = {
                requestParameters: {
                    SearchTerm: SearchTerm,
                    SizeID: SizeID,
                    ColorID: ColorID,
                    CategoryID: CategoryID,
                    TagID: TagID,
                    ManufacturerID: ManufacturerID,
                    MinPrice: MinPrice,
                    MaxPrice: MaxPrice,
                    Rating: Rating,
                    OrderByColumnName : OrderByColumnName,
                    PageNo: PageNo,
                    PageSize: PageSize,
                    recordValueJson: "[]",
                },
            };

            setshowPagination(false);


            const response = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_All_PRODUCTS'], null, param, headers, "POST", true);
            if (response != null && response.data != null) {

                await setProductsList(JSON.parse(response.data.data));
                let AllProducts = JSON.parse(response.data.data);
                await setTotalRecords(parseInt(AllProducts[0]?.TotalRecords ?? 0))
                console.log(JSON.parse(response.data.data));

                if (AllProducts.length > 0) {
                    await setshowPagination(true);
                }

            }


        }

        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));

        // call the function
        getAllProducts().catch(console.error);

        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);

    }, [])

    return (
        <>
            <Helmet>
                <title>{siteTitle} - All Products</title>
                <meta name="description" content={siteTitle + " - All Products"} />
                <meta name="keywords" content="All Products"></meta>
            </Helmet>

            <section className="products-collections-area ptb-60">
                <div className="container-fluid">
                    <div className="section-title">
                        <h2><span className="dot"></span>All Products</h2>
                    </div>

                    <div className="row">


                        <SiteLeftSidebarFilter
                            RowColCssCls={RowColCssCls}
                            setFilterValueInParent={setFilterValueInParent}

                        />

                        <div className="col-lg-9 col-md-12">

                            <ProductsFilterOptions
                                onClick={(e) => { handleGrid(e); }}
                                setPageSizeFromProductFilter={setPageSizeFromProductFilter}
                                setSortByFilter={setSortByFilter}
                                PageNo = {PageNo}
                                PageSize = {PageSize}
                                TotalRecords = {ProductsList!=undefined && ProductsList.length > 0 && ProductsList[0].TotalRecords!=undefined && ProductsList[0].TotalRecords!=0 ? ProductsList[0].TotalRecords : 0}
                            />

                            <div id="products-filter" className={`products-collections-listing row ${gridClass}`}>

                                <ProductsGridTypeOne
                                    ProductsList={ProductsList}
                                    ProductListMainClass={ProductListMainClass}
                                   
                                />
                            </div>

                            {
                                showPagination == true ?
                                    <SitePagination
                                        TotalRecords={TotalRecords}
                                        CurrentPage={PageNo}
                                        PageSize={PageSize}
                                        setCurrentPage={setCurrentPage}
                                    />

                                    :
                                    <>
                                    </>
                            }


                        </div>



                    </div>






                </div>
            </section>



            <BestFacilities />
            <SubscribeNewsLetter />

        </>
    );

}

export default AllProducts;


