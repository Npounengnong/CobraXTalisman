import React, { useEffect, useState } from 'react';
import { MakeApiCallSynchronous , MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import Config from '../../../helpers/Config';
import ProductsGridTypeOne from '../products/ProductsGridTypeOne';
import { useSelector, useDispatch } from 'react-redux';
import rootAction from '../../../stateManagment/actions/rootAction';
import { LOADER_DURATION } from '../../../helpers/Constants';
import { Link } from 'react-router-dom';
    

const RelatedProducts = (props) => {

    const dispatch = useDispatch();
    const [ProductId, setProductId] = useState(props.ProductId);
    const [ProductsList, setProductsList] = useState([]);
    const [ProductListMainClass, setProductListMainClass] = useState("col-lg-3 col-sm-6 col-6");
    const [PaginationInfo, setPaginationInfo] = useState({
        PageNo: 1,
        PageSize: 20,
        TotalRecords: 0
    });



    useEffect(() => {
        // declare the data fetching function
        const getRelatedProductsList = async () => {

             
            const headers = {
                // customerid: userData?.UserID,
                // customeremail: userData.EmailAddress,
                Accept: 'application/json',
                'Content-Type': 'application/json',
                  
            }


            const param = {
                requestParameters: {
                    ProductId: ProductId,
                    PageNo: PaginationInfo.PageNo,
                    PageSize: PaginationInfo.PageSize,
                    recordValueJson: "[]",
                },
            };

            const response = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_RELATED_PRODUCTS_LIST'], null, param, headers, "POST", true);
            if (response != null && response.data != null) {
                await  setProductsList(JSON.parse(response.data.data));
                console.log(JSON.parse(response.data.data));
            }


        }

         //--start loader
         dispatch(rootAction.commonAction.setLoading(true));

        // call the function
        getRelatedProductsList().catch(console.error);

         //--stop loader
         setTimeout(()=>{
            dispatch(rootAction.commonAction.setLoading(false));
        },LOADER_DURATION);

    }, [])

    return (
        <>



            <section className="products-area pt-60 pb-30">
                <div className="container">
                    <div className="section-title">
                        <h2><span className="dot"></span>  Related Products  </h2>
                    </div>
                    <div className="row">
                        <ProductsGridTypeOne
                            ProductsList={ProductsList} 
                            ProductListMainClass = {ProductListMainClass}
                            ProductDetailPageForceUpload = {true}
                        />

                    </div>
                </div>



            </section>

        </>
    );

}


export default RelatedProducts;
