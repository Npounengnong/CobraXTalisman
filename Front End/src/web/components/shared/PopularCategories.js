import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { MakeApiCallSynchronous , MakeApiCallAsync } from '../../../helpers/ApiHelpers';
    
import Config from '../../../helpers/Config';
import { makeAnyStringLengthShort, replaceWhiteSpacesWithDashSymbolInUrl } from '../../../helpers/ConversionHelper';

import CPimg1 from '../../resources/themeContent/images/category-product-image/cp-img1.jpg';
import CPimg2 from '../../resources/themeContent/images/category-product-image/cp-img2.jpg';
import CPimg3 from '../../resources/themeContent/images/category-product-image/cp-img3.jpg';

const PopularCategories = () => {
    const [PopularCategoriesList, setPopularCategories] = useState([]);
    const [adminPanelBaseURL, setBaseUrl] = useState(Config['ADMIN_BASE_URL']);

    useEffect(() => {
        // declare the data fetching function
        const getPopularCategories = async () => {

             
            const headers = {
                // customerid: userData?.UserID,
                // customeremail: userData.EmailAddress,
                Accept: 'application/json',
                'Content-Type': 'application/json',
                  
            }


            const param = {
                requestParameters: {

                    recordValueJson: "[]",
                },
            };

            const response = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_POPULAR_CATEGORIES'], null, param, headers, "POST", true);
            if (response != null && response.data != null) {
                setPopularCategories(JSON.parse(response.data.data));
                console.log(JSON.parse(response.data.data));
            }


        }

        // call the function
        getPopularCategories().catch(console.error);
    }, [])


    return (
        <>
            <section className="category-boxes-area pt-60">
                <div className="container">
                    <div className="section-title">
                        <h2><span className="dot"></span> Popular Categories!</h2>
                    </div>

                    <div className="row">


                        {
                            PopularCategoriesList?.map((item, idx) =>

                                <div className="col-lg-3 col-sm-6">
                                    <div className="category-boxes">
                                        <img src={adminPanelBaseURL + item.AttachmentURL} alt="image" />
                                        <div className="content">
                                            <h3>{makeAnyStringLengthShort(item.Name, 30)}</h3>
                                            <span>{item.TotalProducts} Products</span>
                                            {(() => {
                                       

                                                let allProductsUrl = `/all-products/${item.CategoryID ?? 0}/${ replaceWhiteSpacesWithDashSymbolInUrl(item.Name)}`
                                                return (
                                                    <>
                                                        <Link to={allProductsUrl} className="shop-now-btn">
                                                            Shop Now
                                                        </Link>
                                                    </>
                                                );
                                            })()}




                                        </div>
                                    </div>
                                </div>


                            )}




                    </div>
                </div>
            </section>
        </>
    );

}


export default PopularCategories;
