namespace Walgelijk.Onion.Assets;

public static class BuiltInAssets
{
    public static class Icons
    {
        // Icons from https://tabler-icons.io/

        public static readonly Texture Exit
            = TextureLoader.FromBytes(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADTSURBVFhH7ZYBCsIwDEWLV5pH0vtfwFFfIIWROZtsLUXsg+Bsm/+TOLBpMpl8I+d8l9CvYS7lk7gQL42nLrsh57HJjxdBUilAWAl3EZwVc8kRRGPRrRgkWqFqEZwpnQuhwj+CgFuQvXDBLhCqCrPWtnMLgocGPPfp3ILwzojo27kFA2vYv3MLRttJCKc7v+nn70Cn434CDMa9hAgfGvG8K0y32oBg1YC1PpNAyC3MXttJIBAW5EybSZDY8u/40n3A1bmFnDIJidP3gXFXssnkT0jpDU5tZ5R+bideAAAAAElFTkSuQmCC"));

        public static readonly Texture ChevronDown
            = TextureLoader.FromBytes(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC0SURBVFhH7ZZRCsMgEERtr9Qcqb1tjtRiHNiBYCbqmo98dAcewqC7s36Ij5xzulNPW29TBIgAESAC9AIsxqz65/EUn/AqfI2PeR7eBZ5fzDsgTYMBoF/BEwLNcQZCDdRS+5oBQF1oJAQnh7rBpVnhKegOLE3BSGHX5ESaJ7QauCcn0mygGk1NTqTZoW44NTmR5gD7m4DckxNpDsKbmJqcXP2U8pldbXUrfsURIAJEgH8PkNIGBdYHccXO6zoAAAAASUVORK5CYII="));

        public static readonly Texture ChevronUp
            = TextureLoader.FromBytes(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC+SURBVFhH7ZbRDcMgDERJV0pGatXs1I6UkRpRu7IlRC4BOx/5qE96gpwCPvyBGHLO6UrdZLxMESACRIAIcDbAJPjFV7GTO/ERHuKZgWYHXHwlVDx3hYBmAz05iwtrEFcnoHlAeXItWAcyhYDmDkeFULBy7S7QBPQUcHUCmhWWjc2dgGaBp7WmTkBTGAlzS4U6OO+F/uu6CVdiJl6/rz69iSfBa1mDjBu1HqV6zS4yWtVcH6/iCBABIsC/B0jpC+hypz2+dlYuAAAAAElFTkSuQmCC"));

        public static readonly Texture Check
            = TextureLoader.FromBytes(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADcSURBVFhH7ZWxDcJADEXTIGZgAmAU5gjMQQVzUKaEoZggLeh4vhgpshKExPkK5Cd9Ieycv/+lSBMEQVCDlFKLenTRUj0w3aMHEnot1wFDMX9m62GJVlv+YDZOLksctOUPZjZ5dfPfknNogXZopaWv4PkyyTl4yiNSuqO1lj/Cc+XeOYfPecyALLHR1iT0y75zBizRTaYpszdBvVzyMQySJa4yVZElttrO8L9scgsDZ2+CX5/kFgZP3cQR+SW3YGBv4o1fcgtG9ib8k1swlCU6JJ/Weh+WIAj+mKZ5AbvJp/2PXPNcAAAAAElFTkSuQmCC"));
    }

    public static readonly FixedAudioData Click = new(
        FromB64("Gh+cO+b0Jrv/b2686I2jupZQdbt19fO6Yl2uPPD3WzyK8PW5aqFau3xUd7tM0Bi8kEgvvCaw9jvgBrY8VzpVPAPqg7u8b7Y7Vu" +
            "sTvLQj9bzRrDe7IKIQPJL6YTo6R+67WdscO45K2rvHDd+8vnqeO0RcPryuMCi81+03PdJ6kLpCh6W7iKwTPVu3F72oJSK9bo6lPHiO" +
            "ibyQTk2785k7Pbp7gjuSRnS8B0mZOvhPN727oDu9UbglPVYs+DxKf4o8686cPZT7ijt6RHy99R+avIw32rwJxSq81T1GPbzeXT3Y8H" +
            "s8Sa6PvOONhryawFW9lsAtvR/2xzyWNIU8qGc2Pb5xNz0ShOa83RMUvYxw+bycGAy8jr6oO85INT2Ye3M9Avjyuxu8PryCOje9AX4q" +
            "vQ6lWjwAfb08xh+9uzok3ry0XHg97X/3vEFyIb3Mp709b15evJ7lMjv2Cpe8N+g8va6HEz0KAxa9vKcIPVIomT3k2ci8NQcRvT9oiD" +
            "xKFxA9bxo/vXq4lDyYgbc9eWs6veqNtL3SyIg78vcyPFawA714ssQ8XjhVPXMyM7wR8bK8KsiAuz2qUrxslQg8tPaMPJCbr7tLPtU7" +
            "6vsmvFulCb3mk7K7vgizO1AsGbwMwvi7HswpPBDaKryo1aW8bA6VO5gKpDqlhS47Jv4mvJgN8Ttd8q47yKZrveRW2riTLJu8koaVvY" +
            "PHED38mPM7KcjMu67ZXD2NG6o8GT8HPTREkTzGDAM9EbEVPXV3Fb2M/jy9Zpu2vWhE0b3nxl+9PhMOvVgilDyGRj49St8QPS11sjtw" +
            "91+86YZwu1cxLrxgGgw9BQuuPdAWGD3ViRO8+KiMvEjOVr3gfky9UsjDO0IRDj3CnOo8bIJIPBaQzTyT2r+5LplLvaKRhLw2/8q8NJ" +
            "BCvQntoTyxPDw9GCgaPICX3zwUsgk92D0UO//Plz3crzI9RYg6vYUikrxKDB6+R1AyvvBI/jv5YBk8z7F7PcjJIj62zZQ9D0aFvKQ2" +
            "Fj3KxTY9K0VLvEa3Uj0wcCQ8iCL2vWT6Or7bBIy+fCoLvsZMNT7E9II+SIwaPn6+kj1iaTa8uhsRvjsrb77Mu0W+fsIpPSbKej4mMy" +
            "Y+J7MVPWIvDz2+oG69dAwkvtgIh7rqoRo+wHzbPFSPW73Ubqc9PoLjPaMpmL1KkCm+xxulvbW/LbwKhas7NB+DPeyAJj5IhT0+vgdI" +
            "vEz3ub5nTs6+BDLrvBcxgT70uV0+aGv4PdAAuD2MTlq9KIVBviLAiL2ptwY++MEtPqTXpz0Ra1e9jnS8veYWGDyaF7S8951cvLxV3T" +
            "1S2fu95W+qvvmGHb5r3Jk9BkhfPkIE+D3Un1i9kJpiO0xyZD20olq79lvuvUd1Lb5U9ic9DPN/PlhWKT55UI696PF0vs43Db5OkM09" +
            "qg86Pkz2zz0uMlI9ENZ1Po06iD6kRSe9SMRjvqxOnL1e24I+7iLTvKBPML/2UiW/taWRPWsbVD/hLiQ/aCHJveZPrb7I8YC+vjwAvn" +
            "oAXL6kB969xNeSPiFLyD7zbhs+tAKJvgnbg74lLjo9F46tPY7ZBj086U29nUmSvWuYuzzMxkS9lJmFvlkkPb4l3rc92ySDPryviT5W" +
            "rp4945Mtvd42CL1rg+e9T8ZFvvb3170slBI+G2iAPulYEr1WSdO+JM1xvnSrtj5q8ws/X64vPrF/pb6QcNW+7v1lvQ6jRT5yqYI+aq" +
            "9+PjrMpT24/zi9PtFuvs6zwL5+sAC+hDQwPsgemz7cYCA+ecuDvvJVoL6wV+u9BoEevYrI2D1kxas9hQPWvSxnQ76afQa+VNwJPjaP" +
            "tj5CC7I+u/levWQu5b7O16S+zAGRvQidWT7WpGM+fVSau7Kwmz3kXdI9gzeRPRhdprxEdQ6+IB9iPZ4XHD5+evc9ki+NPRL1Eby2XQ" +
            "Y9kgIGvuSisb7mYW2+QrkaPrR24T6YYT4+Aud4vvK6rL6mIgy+h0fFPRParzznVKe9A+ogPignsT4gLkE+XWxlvfiYLb75bSq9T0bk" +
            "PdZaXL2QROq91L+wPZ/uJD7KcV0+ZPYtPvkAWr5IwDO/SONhv3DW6b7agqw+/IRXP6isJT+eC5k9yv15vsI3d76UUjq+gcWHvUyFhT" +
            "3SthY+1LSiPtEFnz7zy0S6LM8fvo4FGL7yijq+Wtp3vvhXhL7ifey8MmGvPthr7D7ZwhY+Bot4vrrNi7784gS+5eTiPJn2Fz4U9Ts+" +
            "0FiePrf+hj5G8YO9MjCnvj4Mu77Sx/e9OPEQPpC3UT70YV0+VKxKPkaPp7tp0a2++7PpvurZsL5u9mC9oHKKPvwPbD7eohY+HgkDPr" +
            "KUZL24i9a9AlgPvs/CMr5Ws6o8p7v7PTZ12D0Adt49gAbmPCkeFb1u1VS95C6QvR4glLuEmqA9/tSqPbbApz06BIY9XY6CPKYlmbum" +
            "ZvU73zJ/PNhOOj0cAKM92pKUPf7N7T0cChA+ypYKPh/A/j07coK+Uookv2+o/b6bcoC+Io5hPWZIrj72sn0+OHRnPpzOAz7wx3a+iF" +
            "CHvunvXL1gzO09h2GVPliigz6itY096nunuzclQL18xxe+Id8evqizub10FDw8cScoPgaOFD5jGCK94pAuvv3MM74c/KO9oJ7zvBD7" +
            "GrwUrwk+oxBpPt5EHT6ww0E9lD6Tvda/yL268zu9Ihvcvd2vzL0s7Vc9VHz3PYzOBz5YDaA9aXa7vfTLEL6xYZ+9QC94vXTC6Tyb/f" +
            "M9reaQPdBNFD3E3RE9dTqiO4WZsrz0s3q9QnmfvW1b5bwoXHc9YwGxPTENpjyiJvO7qMzdvH/cKr2MtJQ7yBYeu/oVpTze8oU9lUsS" +
            "PCz3kTtKJ8M8cQklvBxo8jtuppI8t0MfvNA4vby8Hyy9dtsCvb8sJz1wtrw9ApePPcy+dD3f3MA8TqOcvSIWUr00B/k8rmaQPNyXj7" +
            "waEZ294jJIvfjMyTzPlGK9mF/NvWK9mbwISVo8NKFLvE9qw7x8uXK8xHMBPFtNZ7uT8sm8bhAqPYrcoj1etys9CjD0PIh0MjyqTSi8" +
            "cP2jvACdu7zoeGc8sn5ZPaAMqDz6eUy9PiI0vQzLlTwJm189tNj6PEli5bweOLy7GuomPYjK47xhdoe9HpJPOxKZ2jwv3aU5t/Udvc" +
            "hZA72FREY9TnBbPdAnsDzODQ09cN7oPDGuDjwBI0O8O7zNOqOzTj2SnnU9ssGWPI9srLyFVYe8cLafvMarlr3G+Ma99lI5vRwtajzW" +
            "ABQ9jOApPExxH71Enwu9UMczPHSsvDyitmg86OlbOrraVrtfHEA96ILFPUSTij0YTgE8He8MvRgSa72UhEu9MBDwvKn8ALxypjU93W" +
            "WpPRt9Tz1s5oY7KLIFvefwbr1Eara8MNvVPCbowzzW20s8BL7JukzBLbzwoHk83gvMPC7oAzzZ3d874Civu4JxwLygZaS8gRChvKLE" +
            "jrw+QMW7aPYIPCIWET3WkiQ91CiLOzFQA73KQzC96s7RvMzfQby2mD+8cHFNPI0yDT0YLto8fEy1PCjEITwq7RO8/GcuugrjsjuAe5" +
            "a73xBGvFxBzrxlK8S8HOMjPEChBz0agZg8j0nLuzB01LyY/wK9TK+NvJ6sV7qT1E88HsbKPB31vjxYIAQ8atgeu/oFBjuUnKE67LwA" +
            "u2FjKjuB4RE7dpJuPFiv8jzNuog8+CpuOjKTPrw3ewC9edELvdmwV7zyEB48bGbCPHtfFz1LCBU9skBBPM6TerxolAK9p6u9vCLBTb" +
            "tatQe8q/mNvPx4Lbyx2gs77gmbPL6R4TxFF6Y8E/93ubB3dLxE4/S7fn4WOynuHjyc6zg8iEXJO8LOeTySv5w8wnPiO6tjHrwmDc+8" +
            "eA29vNLYRLy1jLC7MKafu1unzbvGQN+7z1aOvPzdsLyywmC8LPhBvH6tQrzcCPm7RMbhOTweyjsnvwY88PF+PBvRvDz+woc8kU6BOk" +
            "7zzLsagFK7rjakOyYcjTwMkdA8yeyXPAgt5zpygyy6gncYPFG1FDzqDLm52nAhvDjRI7wdPxG7ZPOgO25RVDw57xQ85P3Yu/0wZrzw" +
            "W5a8O9PivNtc/LzaXJ+82cQ8OcMyOjy1+3o7HFwDvPycN7yUhku8iEBqvB7HV7wPTNm7"),
            44100, 1, 777
        );

    private static float[] FromB64(string s)
    {
        var bytes = Convert.FromBase64String(s);
        var output = new float[bytes.Length / 4];
        int bi = 0;

        for (int i = 0; i < output.Length; i++)
        {
            var next = BitConverter.ToSingle(bytes, bi);
            bi += 4;

            output[i] = next;
        }

        return output;
    }
}
